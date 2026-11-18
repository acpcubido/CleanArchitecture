# Cubido.Template

ACP Cubido's Clean Architecture solution template for .NET. The NuGet package ID is `Cubido.Templates`; its `dotnet new` short name is `cubido-template`.

## Use with Visual Studio

1. In a terminal, install the template package:

   ```shell
   dotnet new install Cubido.Templates
   ```

   You can update the template with `dotnet new update`.

2. Restart Visual Studio if it was open during installation.
3. Select **Create a new project** and search for **ACP Cubido Clean Code Template**.
4. Select the template, choose **Next**, enter the project name and location, then select **Create**.
5. Configure the template options in the wizard. For any options not shown there, or to change the optional components, create the solution with the .NET CLI using the commands above.

The generated solution uses the name you provide.

## Use with the .NET CLI

Install the template from NuGet:

```shell
dotnet new install Cubido.Templates
```

For subsequent template updates, use `dotnet new update`.

Create a solution in a new `MyProject` directory:

```shell
dotnet new cubido-template --name MyProject --output MyProject
```

The template includes Angular, the MCP server, Azure deployment files, and Entra ID authentication by default. To create a starter without those optional parts:

```shell
dotnet new cubido-template --name MyProject --output MyProject --IncludeFrontend false --IncludeMcpServer false --AzureDeployment false --UseEntraIdAuthentication false
```

When keeping the defaults, provide the required values when prompted or as options:

| Option                    | Required when                                          | Example                                |
| ------------------------- | ------------------------------------------------------ | -------------------------------------- |
| `--ClientId`              | Entra ID authentication is enabled (default)           | `00000000-0000-0000-0000-000000000000` |
| `--TenantId`              | Entra ID authentication is enabled (default)           | `00000000-0000-0000-0000-000000000000` |
| `--Audience`              | Entra ID authentication is enabled (default)           | `api://your-api-id`                    |
| `--ScopeId`               | Entra ID authentication and the MCP server are enabled | `api://your-api-id/scope`              |
| `--FriendlyName`          | Azure deployment is enabled (default)                  | `myproject`                            |
| `--RemoteAccessGroupName` | Azure deployment is enabled (default)                  | `My Admin Group`                       |
| `--RemoteAccessGroupSID`  | Azure deployment is enabled (default)                  | `00000000-0000-0000-0000-000000000000` |
| `--WhiteListIpAddress`    | Azure deployment is enabled (default)                  | `1.2.3.4`                              |
| `--FQDN`                  | Azure deployment is enabled (default)                  | `example.com`                          |
| `--BlobContainerName`     | Optional; Azure deployment is enabled                  | `uploads`                              |

For example, include the required authentication values directly in the command:

```shell
dotnet new cubido-template --name MyProject --output MyProject --ClientId "your-client-id" --TenantId "your-tenant-id" --Audience "api://your-api-id" --ScopeId "api://your-api-id/scope" --FriendlyName "myproject" --RemoteAccessGroupName "My Admin Group" --RemoteAccessGroupSID "your-group-sid" --WhiteListIpAddress "1.2.3.4" --FQDN "example.com"
```

Run `dotnet new cubido-template --help` to see all available options. Once created, open the generated `.slnx` solution in Visual Studio or run `dotnet build` from the output directory.

To update the installed template, run `dotnet new update`.

To remove it, run: `dotnet new uninstall Cubido.Templates`
