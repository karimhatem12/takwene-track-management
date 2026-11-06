# Track Management Web (Angular 21)

The front end of Takwene Track Management. Setup, the API and how to sign in are described in the
[main README](../README.md).

```bash
npm install
npm start        # http://localhost:4200 (expects the API on https://localhost:7142)
npm run build    # production build into dist/
```

- `src/app/core`: authentication (service, HTTP interceptor, sign-in/register popup), API models, error messages
- `src/app/features/tracks`: track list (artist/status filters), track detail (DSP distributions, history),
  add/edit track popup, send-to-DSPs popup, status move
- `src/app/features/artists`: artists page and add/edit artist popup
- `src/app/shared`: popup (dialog) shell, status badge and ISRC formatting
- `src/environments`: API base URL

The app was generated without a unit-test setup (`--skip-tests`); the business rules are tested in the
backend (`dotnet test`).
