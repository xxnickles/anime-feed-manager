init
  set token to my @data-passkey-token
  send registerPasskey(token: token) to me
end

on registerPasskey(token) from me
  js (token)
    return new Promise(async (resolve) => {
      try {
        const p = new Passwordless.Client({ apiKey: API_KEY });
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
