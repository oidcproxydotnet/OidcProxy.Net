# Configuring Auth0

OidcProxy.Net only supports the Authorization Code Flow with Proof Key for Client Exchange. That’s why it is important to configure Auth0 in a specific way.

Follow these steps to configure Auth0 correctly:

- Go to https://manage.auth0.com and sign in
- Go to the Applications section in the menu on the left-hand side and click Applications
- Click + Create application in the right upper corner
- Provide a name for your app and select `Regular web applications
- Now, click settings, now you’ll see the following section:

![](https://miro.medium.com/v2/resize:fit:1400/format:webp/1*NmN8wSQKp5ZJ3V5JKi0XFw.png)

- Use this client_id, the secret, and the authority from this screen in the appsettings.json of the spa+bff and the API. Where to use them is described in the main [readme](readme.md).

- Next, configure the redirect_url. When the user has logged into Auth0, Auth0 will redirect the user to this URL. Redirecting will not work unless the redirect URL has been whitelisted. Enter the following url "https://localhost:8444/.auth/login/callback":

![](https://miro.medium.com/v2/resize:fit:1400/format:webp/1*KbMWPT1SPdRTBxy2ndHAWg.png)

- Next, scroll to the `Advanced settings` and configure the `grant_types`. Enable `Authorization Code` and `Refresh tokens`:

![](https://miro.medium.com/v2/resize:fit:1400/format:webp/1*YuIEjPf9JNfqxt1iaMI9sQ.png)

Proceed with the instructions described in [readme.md](readme.md).


