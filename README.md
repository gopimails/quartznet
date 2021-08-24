# quartznet
Quartz net memory leak and recovery check

## Solution creation steps
-  Created a .Net 5 console app with reference to HostedService, Quartz and Serilog.

```
<PackageReference Include="Microsoft.Extensions.Hosting" Version="5.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Configuration" Version="5.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="5.0.0" />
    <PackageReference Include="Quartz" Version="3.3.3" />
    <PackageReference Include="Serilog.AspNetCore" Version="4.1.0" />
```
- Created One hosted service (QuartzHostedService) that fetches all jobs and add it to Quartz scheduler.
- One static instance job (SharedJobRunner) that is responsible for executing the job.
- A shared job factory (SharedJobFactory) that always retuns the SharedJobRunner job.
- Created few jobs that just creates a list of byte, add allocation to it.
`private IList<byte[]> createLeakList = new List<byte[]>();
createLeakList.Add(new byte[1024]);
`
## Containerize the app
- Once the app development is done, we build and publish it.
`dotnet publish -c Release`
- Output will be available in "bin/Release/net5.0/publish".
- Add new file (name : Dockerfile) in the root where csproj is residing
- Add below to docker file

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY bin/Release/net5.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "quartznet.dll"]
```
- Docker file structure
  -  **FROM** specifies the .NET Core 5.0 runtime
  - **COPY** tells Docker to copy the content inside publish folder to App folder in the container
  - **WORKDIR** changes the current directory of container to App
  - **ENTRYPOINT** tells Docker to configure the container to run as an executable
- Build the docker

`docker build -t quartznetleak -f Dockerfile .`
- This will create an image named quartznetleak
### Create and Run container
- `docker create --name qbox quartznetleak` which will create a container named qbox.
- `docker start qbox` will start the container
- `docker logs -f qbox` to see live logs
- More reading [here](/dotnet/core/docker/build-container?tabs=linux).
## Memory leak check
- We update the docker file to have a 