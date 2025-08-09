(() => {
    const pluginName = "content-limit";

    function plugin_default(Alpine) {
        const allowedTypes = ["text", "password", "email", "number", "url", "search", "tel"];
        Alpine.directive(pluginName, (el, { modifiers, expression }, { evaluateLater, effect, cleanup }) => {
            if (el.tagName !== "INPUT" && el.tagName !== "TEXTAREA") {
                console.warn(`[${pluginName}]: The directive "content-limit" can only be used on <input> or <textarea> elements.`);
                return;
            }
            if (el.tagName === "INPUT" && !allowedTypes.includes(el.type)) {
                console.warn(`[${pluginName}]: The directive "content-limit" can only be used on <input> elements with type "text", "password", "email", "number", "url", "search" or "tel".`);
                return;
            }
            const evaluateParams = evaluateLater(expression);
            let listenersToCleanUp = [];
            effect(() => {
                evaluateParams((params) => {
                    params = params || {};
                    try {
                        if (params.regex) {
                            params.regex = new RegExp(params.regex);
                        }
                    } catch (e) {
                        console.error(`[${pluginName}] : Invalid regular expression has been provided in the field with name "${el.name}": ${params.regex}`);
                    }
                    if (params.length !== void 0 && params.length !== null) {
                        params.length = parseInt(params.length, 10);
                        if (isNaN(params.length)) {
                            console.error(`[x-${pluginName}] : Invalid length value has been provided in the field with name "${el.name}". The value must be an integer. Value provided: ${params.length}`);
                            params.length = void 0;
                        }
                    }
                    const isValid = (char) => {
                        if (modifiers.includes("alphanumeric")) {
                            return /[a-zA-Z0-9]/.test(char);
                        }
                        if (modifiers.includes("numeric")) {
                            return /[0-9]/.test(char);
                        }
                        if (modifiers.includes("regex") && params.regex) {
                            return params.regex.test(char);
                        } else {
                            console.warn(`[x-${pluginName}] : The directive "content-limit" has been used with the "regex" modifier but no regular expression has been provided in the field with name "${el.name}".`);
                        }
                        return true;
                    };
                    const enforceConstraints = (value) => {
                        if (params.length !== void 0) {
                            value = value.substring(0, params.length);
                        }
                        return value;
                    };
                    const keypressHandler = (e) => {
                        if (!isValid(e.key)) {
                            e.preventDefault();
                        }
                    };
                    el.addEventListener("keypress", keypressHandler, true);
                    listenersToCleanUp.push(() => el.removeEventListener("keypress", keypressHandler));
                    const inputHandler = (e) => {
                        const newValue = enforceConstraints(e.target.value);
                        if (newValue !== e.target.value) {
                            e.target.value = newValue;
                        }
                    };
                    el.addEventListener("input", inputHandler, true);
                    listenersToCleanUp.push(() => el.removeEventListener("input", inputHandler));
                });
            });
            cleanup(() => {
                listenersToCleanUp.forEach((listener) => listener());
            });
        });
    }
    
    document.addEventListener("alpine:init", () => {
        window.Alpine.plugin(plugin_default);
    });
})();