function login() {
    return {
        alias: '',
        valid: true,
        result: {success: true, unauthorized: false, errors: {}},
        async login(event) {
            const Client = Passwordless.Client;
            const p = new Client({
                apiKey: API_KEY
            });
            try {
                // Option 3: Use an alias specified by the user.   
                const {token, error} = await p.signinWithAlias(this.alias);

                if (error) {
                    if (error.status && error.status === 401 || error.status === 403) {
                        this.result = {
                            errors: error,
                            unauthorized: true,
                            success: false
                        }
                        return;
                    }

                    if (error.status && error.status >= 400) {
                        this.result = {
                            errors: error,
                            unauthorized: false,
                            success: false
                        }
                        return;
                    }
                }


                // Call your backend to verify the token.
                const verifiedUser = await fetch('/verify-signin?token=' + token).then((r) => r.json());
                console.log('verified user', verifiedUser)
                if (verifiedUser.success === true) {
                    document.getElementById('id-value').value = verifiedUser.userId;
                    event.target.submit();
                }
            } catch (e) {
                console.error("Things went bad on sign-in", e);
                this.result = {
                    errors: {ex: [e]},
                    success: false
                }
            }
        }
    }
}