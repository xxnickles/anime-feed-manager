import ky from 'ky';
import { Build } from "@stencil/core";
const baseApi = ky.extend({
  prefixUrl: Build.isDev || Build.isTesting ? 'http://localhost:7071/api' : 'https://animefeedmanager.azure-api.net/api',
  hooks: {
    beforeRequest: [
      addAuthorizationHeaders
    ]
  },
  retry: {
    limit: 5,
    methods: ['get'],
    statusCodes: [413]
  }
})

function addAuthorizationHeaders(request: Request) {
  request.headers.set('Ocp-Apim-Subscription-Key', '3a2588a3f30843fca626c640743529a0');
  request.headers.set('Ocp-Apim-Trace', 'true');
}

export {
  baseApi
}
