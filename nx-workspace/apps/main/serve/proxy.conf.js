const target = `https://localhost:7277`;

const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/swagger",
   ],
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  },
  {
    context: [
      "/hub"
    ],
    target: target,
    secure: false,
    ws: true,
    logLevel: 'debug',
  },
]

module.exports = PROXY_CONFIG;
