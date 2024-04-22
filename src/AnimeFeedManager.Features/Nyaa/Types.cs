namespace AnimeFeedManager.Features.Nyaa;

internal record ShortSeriesTorrent(string Title, string[] Links, string Size);

internal static class Constants
{
    internal const string ScrappingScript =
        """
        () => {
            return [].slice.call(document.querySelectorAll('table.torrent-list tbody tr')).map(row => {
                let columns = row.getElementsByTagName('td')
                let title = columns[1].innerText.replace(/^\d\n/, '');
                let links = [].slice.call(columns[2].getElementsByTagName('a')).map(link => link.href);
                let size = columns[3].textContent;
        
                return {
                    title,
                    links,
                    size
                }
            });
        }
        """;
}