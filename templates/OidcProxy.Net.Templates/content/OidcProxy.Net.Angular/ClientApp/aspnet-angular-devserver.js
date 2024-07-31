/*
This is a workaround.

Unfortunately, Microsoft.AspNetCore.SpaService is end-of-life: https://github.com/dotnet/aspnetcore/issues/12890
However, there is still a demand for a similar developer-experience. That's why OidcProxy.Net.Angular still depends
on Microsoft.AspNetCore.SpaService.

- It is recommended to migrate away from Microsoft.AspNetCore.SpaService, and towards
  Microsoft.AspNetCore.SpaServices.Extensions. Change Program.cs and replace
  `spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");` with `spa.UseAngularCliServer(npmScript: "start");`
  Run `ng serve` from the command prompt.

- This script is a workaround to make Angular 18 compatible with ASP.NET Core. Upgrading from Angular 18 to 19 or above,
  might break this script.

- For better alternatives and feedback, please create an issue here: https://github.com/oidcproxydotnet/OidcProxy.Net/issues

 */
const {exec} = require('child_process');

let process = exec('ng serve', (error, stdout, stderr) => {
  if (error) {
    console.error(error);
    return;
  }

  console.log(stdout);
  console.error(stderr);
});

process.stdout.on('data', (data) => {
  console.log(data);

  if (data.includes('http://localhost:4200/')) {
    // Asp.Net core requires seeing the line 'Open your browser on (http|https)'
    // https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/Spa/SpaServices.Extensions/src/AngularCli/AngularCliMiddleware.cs line 76
    // This is an indicator for ASP.NET that ng serve has complete building the SPA
    // Unfortunately, this only works on Angular < 17.
    // To tell ASP.NET angular is ready, we manually add the message.
    console.log('Relaying "ready" message to ASP.NET Core: open your browser on http://localhost:4200/');
  }
});

process.stderr.on('data', (data) => {
  // Somehow, the `Building...` message makes ASP.NET Core think the Angular build has failed.
  if (!data.includes('Building...')) {
    console.error(data);
  } else {
    console.log(data);
  }
});

process.on('close', (code) => {
  console.log(`child process exited with code ${code}`);
});
