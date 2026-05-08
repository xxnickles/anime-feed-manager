on submit
  halt the event's default

  set aliasInput to the first <input[data-passkey-alias-input]/> in me
  set submitBtn to the first <button[data-passkey-submit]/> in me
  set spinner to the first <span[data-passkey-submit-spinner]/> in me
  set label to the first <span[data-passkey-submit-label]/> in me
  set errorBox to the first <div[data-passkey-error]/> in me
  set unauthBox to the first <div[data-passkey-unauthorized]/> in me
  set inlineError to the first <div[data-passkey-inline-error]/> in me

  set alias to aliasInput.value

  set aliasInput's disabled to true
  set submitBtn's disabled to true
  add .btn-disabled to submitBtn
  remove @hidden from spinner
  set label's textContent to 'Getting Passkey...'

  remove .input-error from aliasInput
  add @hidden to errorBox
  add @hidden to unauthBox
  add @hidden to inlineError

  js (alias)
    return new Promise(async (resolve) => {
      try {
        const p = new Passwordless.Client({ apiKey: API_KEY });
        const { token, error } = await p.signinWithAlias(alias);
        if (error) {
          const unauthorized = error.status === 401 || error.status === 403;
          resolve({
            phase: 'authError',
            unauthorized: unauthorized,
            message: unauthorized ? '' : (error.title || error.message || 'Authentication flow was not completed. Please try again')
          });
          return;
        }
        const verifyRes = await fetch('/verify-signin?token=' + token);
        const { success, userId } = await verifyRes.json();
        if (success) {
          resolve({ phase: 'verified', userId: userId });
        } else {
          resolve({ phase: 'authError', unauthorized: false, message: 'Authentication verification failed' });
        }
      } catch (e) {
        console.error('Things went bad on sign-in', e);
        resolve({ phase: 'exception', unauthorized: false, message: e.message || 'An unexpected error occurred' });
      }
    });
  end
  set result to it

  if result.phase is 'verified'
    set aliasHidden to the first <input[data-passkey-alias]/> in me
    set userIdHidden to the first <input[data-passkey-user-id]/> in me
    set aliasHidden's value to alias
    set userIdHidden's value to result.userId
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
