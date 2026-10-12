# Telemetry Queries

Ad-hoc KQL for App Insights (resource scope, classic tables). Dashboards live in the **AFM Operations** workbook (`deployment/workbooks/afm-operations.workbook.json`); keys follow [telemetry.md](telemetry.md).

## Where things land

| Telemetry | Table | Notes |
|---|---|---|
| Domain span (`Tv.Library.Import`, …) | `dependencies`, `type == "InProc"` | `operation_Name` is empty; use `name` |
| `send {queue}` | `dependencies`, `type == "azure_storage_queue"` | `target` = queue host + queue |
| `process {queue}` | `requests` | Producer link in `customDimensions["_MS.links"]` |
| AnimeSchedule / other HTTP calls | `dependencies`, `type == "Http"` | |
| Log | `traces` | Template args and scope properties in `customDimensions` under their dotted key |
| Log with an exception | `exceptions` | `outerMessage` is the exception message, not the log text |

- Each queue hop is its own operation: walk hops through `_MS.links`, or join send and process rows on `messaging.message.id`.
- A log's `operation_ParentId` is the span that flushed it, so it joins to `dependencies.id` / `requests.id`.

## Follow one message

```kusto
let messageId = "<messaging.message.id>";
union
    (dependencies | where name startswith "send "),
    (requests | where name startswith "process ")
| where timestamp > ago(7d)
| where trim('"', tostring(customDimensions["messaging.message.id"])) == messageId
| project timestamp, itemType, name, success, duration, cloud_RoleName, operation_Id, id
| order by timestamp asc
```

## Everything for an operation, plus its direct hops

```kusto
let op = "<operation_Id>";
let hops = requests
    | where timestamp > ago(7d)
    | mv-expand link = todynamic(tostring(customDimensions["_MS.links"]))
    | where tostring(link.operation_Id) == op
    | distinct operation_Id;
let ops = union (print operation_Id = op), hops;
union requests, dependencies, traces, exceptions
| where timestamp > ago(7d)
| where operation_Id in (ops)
| project timestamp, operation_Id, itemType, name, message, outerMessage, success, severityLevel, id, operation_ParentId
| order by operation_Id asc, timestamp asc
```

Repeat with a hop's `operation_Id` to walk further down (library refresh → series updated → auto-subscribe).

## Producer of a hop

```kusto
let op = "<operation_Id of the process request>";
requests
| where timestamp > ago(7d)
| where operation_Id == op and name startswith "process "
| mv-expand link = todynamic(tostring(customDimensions["_MS.links"]))
| project name, ProducerOperation = tostring(link.operation_Id), ProducerSpan = tostring(link.id)
```

## Logs for one series or user

```kusto
let key = "series.id";   // or "user.id", "series.season"
let value = "<value>";
union traces, exceptions
| where timestamp > ago(7d)
| where tostring(customDimensions[key]) == value
| project timestamp, itemType, severityLevel, message, outerMessage, operation_Id,
    Operation = tostring(customDimensions["operation.name"])
| order by timestamp desc
```

## Scrapes that skipped pages

```kusto
traces
| where timestamp > ago(30d)
| where toint(customDimensions["scrape.pages_failed"]) > 0
| project timestamp, Query = tostring(customDimensions["scrape.query"]),
    PagesFailed = toint(customDimensions["scrape.pages_failed"]), Reason = tostring(customDimensions["domain_error.message"]),
    operation_Id
| order by timestamp desc
```

## Rows per function invocation

Shows which rows the Functions host and worker emit (host span, worker `Invoke`, `process {queue}`), so invocation counts can be built on the right one.

```kusto
union requests, dependencies
| where timestamp > ago(1d)
| where itemType == "request" or name == "Invoke"
| summarize Rows = count() by cloud_RoleName, itemType, name, type
| order by cloud_RoleName asc, Rows desc
```
