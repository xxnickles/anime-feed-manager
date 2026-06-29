-- Token-bearing state: lazily import the Passwordless ESM client and create the new passkey
-- credential for the existing user, then reveal success/error. "Try again" reloads to re-fetch a token.
init
  set token to my @data-passkey-token
  set apiKey to my @data-passkey-api-key
  set moduleUrl to my @data-passkey-module

  js (token, apiKey, moduleUrl)
    return new Promise(async (resolve) => {
      try {
        const { Client } = await import(moduleUrl);
        const p = new Client({ apiKey });
        const { token: credential, error } = await p.register(token);
        if (credential) {
          resolve({ ok: true });
        } else {
          resolve({ ok: false, error: (error && (error.title || error.message)) || 'Passkey registration failed' });
        }
      } catch (e) {
        console.error('Passkey registration error:', e);
        resolve({ ok: false, error: e.message || 'An unexpected error occurred' });
      }
    });
  end
  set result to it

  add @hidden to the first <div[data-passkey-loading]/> in me
  if result.ok
    remove @hidden from the first <div[data-passkey-success]/> in me
  else
    set errEl to the first <div[data-passkey-error]/> in me
    set msgEl to the first <p[data-passkey-error-message]/> in errEl
    set msgEl's textContent to result.error
    remove @hidden from errEl
  end
end

on click from <button[data-passkey-retry-reload]/> in me
  call window.location.reload()
end
