import ky from 'ky';

const baseApi = ky.extend({
  prefixUrl: 'https://animefeedmanager.azure-api.net/api',
  hooks: {
    beforeRequest: [
      addAuthorizationHeaders
    ]
  },
  timeout: 15000
})

function addAuthorizationHeaders(request: Request) {
  request.headers.set('Ocp-Apim-Subscription-Key', '3a2588a3f30843fca626c640743529a0');
  request.headers.set('Ocp-Apim-Trace', 'true');
}

export {
  baseApi
}
