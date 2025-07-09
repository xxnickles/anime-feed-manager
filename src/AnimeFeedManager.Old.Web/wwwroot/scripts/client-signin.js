import {Client} from 'https://cdn.passwordless.dev/dist/1.1.0/esm/passwordless.min.mjs';

export async function signIn(email) {

    // Instantiate a passwordless client using your API public key.
    const p = new Client({
        apiKey: API_KEY
    });

    try {
        // Option 3: Use an alias specified by the user.   
        const {token, error} = await p.signinWithAlias(email);

        if (error) {
            console.error(error);
        }

        // Call your backend to verify the token.
        const verifiedUser = await fetch('/signin?token=' + token).then((r) => r.json());
        if (verifiedUser.success === true) {
            console.info("SignIn successful", JSON.stringify(verifiedUser));
        }
    } catch (e) {
        console.error("Things went bad on sign-in", e);
    }

}
