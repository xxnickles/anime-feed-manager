-- Intercepts the form submit: lazily import the Passwordless ESM client (data-attrs carry the public
-- key + module url), sign in with the typed email, then on success put the auth TOKEN into the hidden
-- field and send `loginComplete` (the form's htmx trigger → POST /login, where the server verifies the
-- token and derives the identity). Failures re-enable the form and surface an inline / unauthorized /
-- error message.
on submit
  halt the event's default

  set aliasInput to the first <input[data-passkey-alias-input]/> in me
  set submitBtn to the first <button[data-passkey-submit]/> in me
  set spinner to the first <span[data-passkey-submit-spinner]/> in me
  set label to the first <span[data-passkey-submit-label]/> in me
  set errorBox to the first <div[data-passkey-error]/> in me
  set unauthBox to the first <div[data-passkey-unauthorized]/> in me
  set inlineError to the first <div[data-passkey-inline-error]/> in me

  set alias to aliasInput's value
  set apiKey to my @data-passkey-api-key
  set moduleUrl to my @data-passkey-module

  set aliasInput's disabled to true
  set submitBtn's disabled to true
  add .btn-disabled to submitBtn
  remove @hidden from spinner
  set label's textContent to 'Getting passkey...'

  remove .input-error from aliasInput
  add @hidden to errorBox
  add @hidden to unauthBox
  add @hidden to inlineError

  js (alias, apiKey, moduleUrl)
    return new Promise(async (resolve) => {
      try {
        const { Client } = await import(moduleUrl);
        const p = new Client({ apiKey });
        const { token, error } = await p.signinWithAlias(alias);
        if (error) {
          const unauthorized = error.status === 401 || error.status === 403;
          resolve({ ok: false, unauthorized: unauthorized, message: unauthorized ? '' : (error.title || error.message || 'Authentication flow was not completed. Please try again') });
          return;
        }
        resolve({ ok: true, token: token });
      } catch (e) {
        console.error('Things went bad on sign-in', e);
        resolve({ ok: false, unauthorized: false, message: e.message || 'An unexpected error occurred' });
      }
    });
  end
  set result to it

  if result.ok
    set tokenHidden to the first <input[data-passkey-token-field]/> in me
    set tokenHidden's value to result.token
    send loginComplete to me
  else
    set aliasInput's disabled to false
    set submitBtn's disabled to false
    remove .btn-disabled from submitBtn
    add @hidden to spinner
    set label's textContent to 'Login'
    if result.unauthorized
      remove @hidden from unauthBox
    else
      set msgEl to the first <span[data-passkey-error-message]/> in errorBox
      set msgEl's textContent to result.message
      remove @hidden from errorBox
      remove @hidden from inlineError
      add .input-error to aliasInput
    end
  end
end
