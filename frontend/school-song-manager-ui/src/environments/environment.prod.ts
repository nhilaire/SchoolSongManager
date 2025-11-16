export const environment = {
  production: true,
  apiUrl: (globalThis as any)?.process?.env?.['API_URL'] || 'http://localhost:5168'
};