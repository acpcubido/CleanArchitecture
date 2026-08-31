[[_TOC_]]

# .NET

## ⚔️ Cryptographic Attacks

Unsichere kryptographische Algorithmen sind anfällig für Rainbow Tables oder gegenüber Kryptoanalyse. Entsprechend bedienen wir uns nur von derzeitig sicheren Algorithmen:

- **Symmetrisch** (dasselbe Passwort für die Ver- und Entschlüsselung)\
[AES](https://learn.microsoft.com/de-de/dotnet/api/system.security.cryptography.aes?view=net-10.0) oder [3DES](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.tripledes?view=net-10.0). Modus: CTR oder GCM. Padding: PKCS
- **Asymmetrisch**\
[RSA](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsa?view=net-10.0) mit [OAEP](https://learn.microsoft.com/de-de/dotnet/api/system.security.cryptography.rsacryptoserviceprovider.encrypt?view=net-10.0) Padding, DH, oder EC
- **Streams** (kein definiertes Ende der Datenübertragung)\
[ChaCha20](https://learn.microsoft.com/de-de/dotnet/api/system.security.cryptography.chacha20poly1305?view=net-10.0) oder XChaCha20
- **Hashes**\
Keccak, [SHA3](https://learn.microsoft.com/de-de/dotnet/api/system.security.cryptography.sha3_512?view=net-10.0), [BLAKE3](https://www.nuget.org/packages/Blake3), oder [Argon2](https://www.nuget.org/packages/Konscious.Security.Cryptography.Argon2)

Es ist auch nicht an der Blocksize und der Keysize zu sparen, sofern es keinen guten Grund dafür geben sollte. In der Regel spricht nichts gegen einen 256bit Schlüssel bei AES, oder einem 4096bit Schlüssel bei RSA. 

- Verwende [RandomNumberGenerator](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator?view=net-10.0) für Salts, Pepper und andere Entropien
- Verwende [CryptoStream](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.cryptostream?view=net-10.0) für Streams
- Verwende [KeyedHashAlgorithm](https://learn.microsoft.com/de-de/dotnet/api/system.security.cryptography.keyedhashalgorithm?view=net-10.0) für HMACs
- Verwende [RsaCryptoServiceProvider](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=net-10.0) für digitale Signaturen
- Verwende [X509Certificate](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-10.0) für Zertifikate
- Speichere asymmetrische Schlüssel in [CspParameters](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.cspparameters?view=net-10.0)

## ⚔️ Token Prediction Attack

Stellen wir einen Token aus (Session, Transaction,...) und verwenden hierfür eine GUID, könnte, aufgrund der Deterministik, nachfolgende Tokens berechnet werden.

```csharp
var rng = new RandomNumberGenerator();
bytes[] key = new bytes[32];
rng.GetBytes(key);

string sessionId = BitConverter.ToString(key);
```

**Warum**: Nachfolgende Tokens sind nicht mehr deterministisch.

## ⚔️ File Tampering

Liefern oder verarbeiten wir eine Datei, könnte diese während dem Transport verändert worden sein. Hashed message authentication codes (HMACs) sind dazu da, um zu garantieren, dass die Datei nicht unautorisiert manipuliert wurde.

- [System.Security.Cryptography.Xml](https://learn.microsoft.com/de-de/dotnet/standard/security/how-to-sign-xml-documents-with-digital-signatures) für XML Signaturen
- [System.Security.Cryptography](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsa.signdata?view=net-10.0) für Binärdaten

⚠️ Eine Signatur gibt keine Auskunft über den Sender. Hierfür gibt es wiederum digitale Zertifikate.

```csharp
byte[] data = File.ReadAllBytes(...);
byte[] signature = null;
RSAParameters publicKey = null;

// sign
var signServiceProvider = new RSACryptoServiceProvider();
signature = signServiceProvider.SignData(data, HashAlgorithmName.SHA256);
publicKey = signServiceProvider.ExportParameters(false); // dont include the primary key

// transmit: data, signature, and the publicKey

// validate
var validateServiceProvider = new RSACryptoServiceProvider();
validateServiceProvider.ImportParameters(publicKey);
bool isValid = validateServiceProvider.VerifyData(data, signature, HashAlgorithmName.SHA256);
```

## ⚔️ Log Tampering

Beachte folgende Punkte beim Loggen von Informationen:

- Gib keinen Aufschluss darüber wo und wie geloggt wird
- Eingabedaten des Benutzers nicht direkt abspeichern
- Sichere die Logdatei mit einer vernünftigen ACL ab
- Verwende kein Dateiformat was vom Webserver ausgeliefert werden kann
- Verwende absolute Pfade zur Logdatei
- Speichere keine sensiblen Daten ab. (Passwörter, Zertifikate, Code,...)
- ⚡ XXE injection: Verwende kein XML als Dateiformat
- ⚡ Dos attack: Verwende Throttling
- ⚡ Dos attack: Definiere Dateigrößenlimits
- ⚡ Dos attack: Lege Logfiles optimalerweise auf einer eigenen Partition ab

## ⚔️ SQL-Injection

Das EntityFramework arbeitet bereits mit parametrisierte Queries, somit sind dort keine weiteren Maßnahmen notwendig. ⚠️ Das folgende Beispiel gilt auch für stored procedures.

```csharp
SqlCommand command = "INSERT INTO ... values (@id, @name, @value);"
command.Parameters.Add(new SqlParameter("@id", id));
command.Parameters.Add(new SqlParameter("@name", name));
command.Parameters.Add(new SqlParameter("@value", value));
```

**Warum**: Angreifer sind sehr kreativ und nutzen oft andere Encodings, welche eine naive Eigenüberprüfung der Parameter in der Regel nicht standhält.

## ⚔️ Privilege escalation

Es gilt für jede Ressource: **least-privilege**. Es ist ein User zu erstellen, welcher auf die benötigte Ressource die nötigste Zugriffsberechtigung erhält. Dies bezieht sich auf diverse Ressourcen. Zum Beispiel aber nicht ausschließlich:

- Datenbank: Read/Write/Execute permissions auf Datenbank und Tabellenebene. Arbeiten mit Views oder Prozeduren ebenso möglich
- Dateisystem: Zugriffsbereich und Zugriffsart einschränken (Jailing)
- Portbesetzung: Welche Ports dürfen, wenn überhaupt, geöffnet werden
- ...

**Warum**: Durch eine Sicherheitslücke in der Applikation erhält der Angreifer automatisch die Berechtigung des ausführenden Applikationsusers. Wir wollen dem Angreifer so wenig Zugriff auf das System wie nur möglich vererben.

## 🛡 Storing secrets

- **Windows Data Protection API**\
Die Datei mit dem Geheimnis kann direkt im Filesystem abgelegt werden. Je nach Speicherort regelt die ACL bereits den Dateizugriff out-of-the-box. Der Inhalt der Datei wird mit einem Schlüssel aus der [Windows Data and Protection API](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection) ver- bzw. entschlüsselt. Optional kann zum generierten Schüsselt der WDPAPI ein applikationsspezifischer Schüssel hinzugefügt werden, um vor Zugriffen anderer Applikationen zu schützen.
- **Registry**\
Der Registryeintrag kann über [ACL Policies so eingeschränkt werden](https://learn.microsoft.com/en-us/dotnet/api/system.security.accesscontrol.registryaccessrule?view=net-10.0), dass nur ein bestimmter Benutzer (und der Administrator) Zugriff darauf hat. Wie bei WDPAPI ist auch hier anschließend der Inhalt zu verschlüsseln.
- **Datenbank**\
Der Zugriff auf die "Secrets" Tabelle kann auf wenige Benutzer oder Service-User eingeschränkt werden. Column-Encryption ist zu aktivieren.

## 🛡 Cookie: Hardening

```csharp
var cookie = new HttpCookie();
cookie.HttpOnly = true;
// ❌
cookie.Expire = DateTime.Now.AddDays(30);
```

**Warum**: Cookie darf nur der Browser auslesen, JavaScript allerdings nicht.\
**Warum**: Nachdem der Browser geschlossen wurde, wird das Cookie automatisch gelöscht.

## ⚔️ Information Leakage by Exceptions

Implementiere eigene exception pages, da Standardseiten oft zu viel Informationen beinhalten. Schreibe stattdessen den Fehler in die Logs und füge eine Trace-ID für die Nachverfolgbarkeit bei der eigenen exception page ein.

Definiere eigene Fehlermeldungen, anstatt `Exception.Message` zu verwenden. Halte diese möglichst generisch. Beispiel: "Datei konnte nicht verarbeitet werden" anstatt: "External XML API caused an error"  
-> Ein Angreifer braucht nicht zu wissen, dass ein Input hier möglicherweise an eine externe API gesendet wird und welches Format hierfür verwendet wurde.

```csharp
// ❌
return exception.ToString();
return exception.StackTrace;
return exception.Message.ToString();
// ✅
// Custom error-page mit Trace-ID
```

## 🛡 SAST

Die meisten Sicherheitslücken können bereits während der Entwicklung oder nach Projektabschluss gefunden und behoben werden. Mit dieser Checkliste werden bereits die meisten Sicherheitslücken abgedeckt:

- Besteht eine sichere Verbindung zum Client?
- Sind alle Daten Ein- und Ausgänge mit Authentication und Authorization abgesichert?
- Werden alle Benutzereingaben Serverseitig bereinigt und sicher abgelegt?
- Gibt es neben Clientseitiger Validierung auch eine robuste Serverseitige Validierung
  - Pflichtfelder
  - Format Vorgaben
  - Eingabe innerhalb möglicher Bereiche (Achtung C# überprüft bei int -> Enum Serialisierung nicht ob diese gültig sind)
  - Maximale Länge
- Wie verhält sich die Software bei Laufzeitfehlern?
- Welche Program- und Systeminformationen werden nach außen getragen?
- Wurden aktuelle Bibliotheken verwendet und sind bereits CVEs dazu bekannt?
- "Da wird schon keiner draufkommen" ist keine Lösung. Es ist nur eine Frage der Zeit!
- Wurden keine Geheimnisse im Source-Code hinterlegt?
- Wurden alle Entwicklungsunterstützungen (eigene Endpunkte, statisch hinterlegter Testuser, Direktzugänge zu Ressourcen) beim Release entfernt?

Bei einem Security Code Review NUR sicherheits relevante Dinge Betrachten. Implementierungsdetails oder Performancethemen haben zu diesem Zeitpunkt keine relevanz und verwässern die Analyse nur.

# ASP NET Core

## ⚔️ Directory Traversal Attack - 1

Folgende Konfiguration ist für `wwwroot` nicht notwendig, da dies der Standardeinstellung von `UseStaticFiles()` entspricht.

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "data")),
    RequestPath = "/data"
});
```

**Warum**: Böswillige Anfragen, welche sich auf einen anderen physicalischen Pfad beziehen würden, werden von .NET nicht aufgelöst.

## ⚔️ Directory Traversal Attack - 2

```csharp
// ❌
builder.Services.AddDirectoryBrowser();
app.UseDirectoryBrowser();
// ✅
app.UseDirectoryBrowser(new DirectoryBrowserOptions()
{
    FileProvider = new PhysicalFileProvider(...),
    RequestPath = "/kittensWithHats"
});
```

**Warum**: Es besteht zu 99% kein Grund alle Dateien und Ordner der Öffentlichkeit bereitzustellen.\
**Warum**: Falls doch, zumindest einen eigenen Pfad für die Inhalte definieren.

## ⚔️ MIME-type Confusion Attack

```csharp
// ❌
app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "image/png"
});
// ✅
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".exotic"] = "application/x-msdownload";
app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider
});
```

**Warum**: Wenn User die Binärdaten selbst über einen upload zur Verfügung stellen, kann sich Schadsoftware dahinter verbergen.\
**Warum**: Der Browser muss bei unbekannten Dateiformate Content sniffing betreiben. D.h. der Dateityp wird erruiert und anschließend eingebunden. Im Falle einer Javascript Datei wird einfach der Code ausgeführt.

## ⚔️ Content Sniffing Attack

Stimmt der Zieltyp und MIME type nicht überein oder wurde keiner definiert, so versucht der Browser selbst den typ aufzulösen. Z.b. `<link href="..." />` statt `<link rel="stylesheet" href="..." />`

```csharp
app.Use(async (context, next) =>
{
    // adapt as needed
    context.Response.Headers.Add("X-Content-Type-Options", "NOSNIFF");
    await next();
});
```

**Warum**: Bei der Auflösung des Types könnte bereits Schadsoftware ausgeführt werden.

## ⚔️ XSS: Content Security

Hier werden alle `src` Quellen verboten, außer die der eigenen Website. Somit können Inhalte, welche von dritten eingeschleust wurden, vom Browser automatisch nicht geladen.

```csharp
app.Use(async (context, next) =>
{
    // adapt as needed
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'none'; script-src 'self';");
    await next();
});
```
**Warum**: Externe Inhalte werden vom Browser automatisch blockiert und nicht geladen.

## ⚔️ XSS: Content Protection

Heutzutage nicht mehr relevant, könnte aber, je nach [Target-Browser](https://caniuse.com/?search=x-xss) und dessen Version, dennoch benötigt werden.

⚠️ **ACHTUNG** Dieser Header sollte nur verwendet werden, wenn die Anwendung noch mit IE verwendet wird. Moderne Browser unterstützten diesen Header nicht mehr, da er auch für Angriffe ausgenutzt werden kann.

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-XSS-Protection", "1");
    await next();
});
```
**Warum**: Externe Inhalte werden vom Browser automatisch blockiert und nicht geladen.

## 🛡 XSS countermeasures

- **Sanitizing**\
[NuGet HtmlSanitizer](https://www.nuget.org/packages/HtmlSanitizer) verwenden, um User-Input von HTML und JS Elementen zu bereinigen.\
**Warum**: Bei der Wiederausgabe des Inputs könnte sonst HTML/JS ausgegeben werden.\
**Warum**: Angebundene Fremdsysteme könnten den Input falsch (oder gar nicht) encodieren.
- **URL encoding**\
[HttpUtility.UrlDecode](https://learn.microsoft.com/en-us/dotnet/api/system.web.httputility.urldecode?view=net-9.0) bzw. [WebUtility.Decode](https://learn.microsoft.com/en-us/dotnet/api/system.net.webutility?view=net-9.0) um HTML und JS sicher abzulegen.\
**Warum**: Eine 1 zu 1 Übernahme der Parameter könnte XSS ermöglichen.\
**Warum**: Eigene Implementierungen können rasch umgangen werden. (Double URL encoding, Hex encoding, URL encoding oder alles kombiniert)\
- **HTML encoding**\
[HttpUtility.HtmlDecode](https://learn.microsoft.com/en-us/dotnet/api/system.web.httputility.htmldecode?view=net-9.0) bzw. [WebUtility.HtmlDecode](https://learn.microsoft.com/en-us/dotnet/api/system.net.webutility?view=net-9.0) um HTML und JS sicher abzulegen.\
**Warum**: Eine 1 zu 1 Übernahme der Parameter könnte XSS ermöglichen.\
**Warum**: Eigene Implementierungen können rasch umgangen werden. (Double URL encoding, Hex encoding, URL encoding oder alles kombiniert)

## ⚔️ Clickjacking

Die Applikation könnte versteckt (z.B. `opacity:0`) als overlay auf einer Seite als iFrame eingebunden werden. Der Benutzer würde somit unbewusst mit dem iFrame interagieren, sobald ein Element auf der Seite angeklickt wird.

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("Frame-Ancestors", "NONE");
    await next();
});
```

**Warum**: Die Seite kann nicht mehr als iFrame eingebunden werden.

## ⚔️ Parameter Tampering

Parameter sollten immer auf deren Gültigkeit und Authenzität überprüft werden. Vor Aktionen sollte immer überprüft werden, ob der Benutzer die notwendigen Rechte besitzt

```csharp
// ❌
var username = model.username;
var id = model.id;
var url = model.url;
// ✅
var username = userManager.GetCurrentUser().username;
var id = secret.Decrypt(model.id);
var url = secret.ValidateHmac(model.url);

if (!userManager.CanAccess(username, id))
{
    return Forbidden();
}

userManager.DoSeriousThings(id, username, url);
```

**Warum**: Jeder Endpoint kann mit beliebigen Daten aufgerufen werden. Es besteht _kein Vertrauensgrundsatz_. \
**Warum**: Manche Parameter kommen bereits vom Back-End. Um Serverdaten vor Manipulation zu schützen, könnte eine Verschlüsselung eingeführt werden.\
**Warum**: Nicht vertrauliche Daten können mit einer HMAC versehen werden.

## ⚔️ Blacklist Evasion

Lassen wir nur einen bestimmten Input im back-end zu, so kommen `blacklist` bzw. `whitelist` Filter zum Einsatz.

```csharp
// ❌
if(blacklist.Contains(input))
    throw new UnauthorizedException();
// ✅
if(!whitelist.Contains(input))
    throw new UnauthorizedException();
```

**Warum**: Es ist zeitaufwendig bis unmöglich, alle möglichen `blacklist` Wertigkeiten zu definieren\
**Warum**: Es gibt einfach zu viele Encodings und code-pages\
**Warum**: Im Zweifel immer `Deny`

## ⚔️ Impersonation Attack

Gruppen- oder Benutzerbezogene Daten sollten nicht über die Berechtigung des ApplicationPools bezogen oder manipuliert werden.

```csharp
// ❌
File.ReadAllLines(...);
// ✅
var user = (WindowsIdentity)context.User.Identity!;
await WindowsIdentity.RunImpersonatedAsync(user.AccessToken, () => File.ReadAllLines(...));
```

**Warum**: Im Scope des Applicationpools könnte der Applikationsuser auf Ressourcen (SMB, AD, Dateien,...) zugreifen, obwohl dessen entsprechende Berechtigungen fehlen.

## ⚔️ CSRF

Wird der Auth-Token in den Cookies gespeichert und `SameSite=Strict` ist nicht anwendbar (z.B. OpenID mit cross-site posts), ist der XSRF-Token ein absolutes muss.

```csharp
[ValidateAntiForgeryToken]
public async Task MyEndpoint() { ... }
```

```csharp
public void Configure(IApplicationBuilder app, IAntiforgery antiforgery)
{
    app.Use(next => context =>
    {
        // The request token can be sent as a JavaScript-readable cookie, 
        // and Angular uses it by default.
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, 
            new CookieOptions() { HttpOnly = true });

        return next(context);
    });
}

public void ConfigureServices(IServiceCollection services)
{
    services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
}
```

**Warum**: Der Browser schickt das Auth-Cookie bei einer Request automatisch mit\
**Warum**: Der XSRF-Token muss erst abgeholt werden, um diesen anschließend mitsenden zu können. Eine WebApp die über einen `iFrame`oder dergleichen eingebunden wurde, hat somit einen sicheren zweiten Faktor.

⚠️ Ohne die richtige CORS Einstellung ist weiterhin CSRF möglich!

## ⚔️ Cross-Origin attack

Der Browser hat eine Same Origin Policy, sodass Anfragen an andere Domains ungleich der eigenen Origin automatisch blockiert werden. CORS lockert dies auf, kann aber mit der falschen Konfiguration zu einer großen Angriffsfläche führen. Verwende das [EnableCors Attribute](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-9.0#enable-cors-with-attributes) um für verschiedene Endpoints verschiedene CORS Einstellungen vorzunehmen.

```csharp
// ❌
app.UseCors(builder => builder.AllowAnyOrigin());
// ✅
app.UseCors(builder => builder.WithOrigins("mydomain.com"));
```

**Warum**: Preflight requests liefern dem Browser zurück, ob der Endpoint überhaupt von seinem aktuellen Origin aus angesteuert werden darf. ⚠ Dies verhindert keine API Aufrufe, sondern ist nur ein Hinweiß für den Browser.

## ⚔️ MITM: Transport Layer Security

Sofern nicht bereits im Reverse-Proxy geregelt, muss die WebApp das Verwenden eines SSL Zertifikates erzwingen.

```csharp
builder.Services.AddHttpsRedirection();
app.UseHttpsRedirection();
```

**Warum**: Verhindert Man-in-the-middle attacks\
**Warum**: Verhindert Klartext bei Network Sniffing Attacks

## ⚔️ MITM: HTTP Strict Transport Security preload submission

Die WebApp sollte in die [HSTS Preload List](https://hstspreload.org/?domain=push-force.dev#submission-form) aufgenommen werden.

**Warum**: Es wird bereits direkt im Browser hinterlegt, dass die WebApp mit HTTPS angesteuert werden soll. Dadurch wird ein MITM attack bereits vor der ersten Abfrage verhindert.

## ⚔️ MITM:  HTTP Strict Transport Security header

```csharp
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromSeconds(31536000);
    options.ExcludedHosts.Add("example.com");
    options.ExcludedHosts.Add("www.example.com");
});
```

**Warum**: Damit sich der Browser auch in Zukunft merkt, dass die Seite nur mit HTTPS angesteuert werden soll.
**Warum**: [HSTS Preload](https://hstspreload.org) akzeptiert nur Einträge mit `max-age: 31536000` (1 Jahr)

## 🛡 Session: Hardening

Sessions support kann über ein [Nuget](https://www.nuget.org/packages/Microsoft.AspNetCore.Session/) hinzugefügt werden.

```csharp
// Session benötigen einen "distributed cache"
services.AddDistributedMemoryCache();
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "myapp";
}

app.UseSession();
```

Checklist für sichere Sessions:
- Speichere die Sessions immer am Server
- Lösche die Session wenn von HTTP auf HTTPS umgeleitet wird
- Möglichst kurze Gültigkeit
- Lösche die Session wenn diese nicht mehr gebraucht wird
- Generiere die Session neu, wenn die Berechtigung sich ändert (z.B. nach einem Login)
- Setze `HttpOnly` für das session-cookie
- Verwende den session-identifier kein zweites mal
- Session-Token nicht als Auth-Token verwenden
- Falls authentifiziert: Überprüfe die Zusammengehörigkeit der Session und des users

**Warum**: Das Framework kümmert sich um den session-state und assoziiert automatisch jeden User mit seinem eigenen session-object.

Neben dem `DistributedMemoryCache` gibt es auch den `DistributedSqlServerCache`. Dieser ist vorzuziehen, wenn Sessions über einen Server Neustart bestehen sollen oder die Anwendung auf mehreren Maschinen läuft (Web-Farm, Kubernetes)

## 🛡 Storing secrets: Locally

Das abzulegende Geheimnis wird über die WDPAPI verschlüsselt und auf dem Dateisystem abgelegt. Es kann für App- bzw. Benutzerübergreifenden Zugriff entsprechende Datei bzw. Ordnerberechtigungen festgelegt werden.

```csharp
var encryptionConfig = new AuthenticatedEncryptorConfiguration()
{
    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
    ValidationAlgorithm = ValidationAlgorithm.HMACSHA512
};

builder.Services.AddDataProtection()
    .ProtectKeysWithDpapiNG()
    .UseCryptographicAlgorithms(encryptionConfig)
    .SetApplicationName("myappname")
    .AddKeyManagementOptions(options => options.NewKeyLifetime = TimeSpan.FromDays(90));
```

`MyClass` konsumiert `IDataProtectionProvider`
```csharp
public class MyClass(IDataProtectionProvider protectionProvider, IOptions<MyClassOptions> options)
{
    public void DoThings()
    {
        byte[] dataToProtect = //...
        string purpose = options.DataProtectionScope;
        var protector = protectionProvider.CreateProtector(purpose);
        byte[] protectedData = protector.Protect(dataToProtect);
    }
}
```

**Warum**: Datei befindet sich im user-scope und die ACL regelt bereits die Zugriffsberechtigung\
**Warum**: Sollte eine andere App oder der Administrator auf die Datei zugreifen, ist diese verschlüsselt\
**Warum**: Der symmetrische Schlüssel ist selbst für uns unbekannt

## 🛡 Storing secrets: Remotely

Secrets sollten im KeyVault abgelegt und nur über die managed identity abrufbar sein. Ist dies nicht möglich, können wir - wie bereits bei _Storing secrets: Locally_ - das abzulegende Geheimnis über die WDPAPI verschlüsseln und online ablegen.

```csharp
var encryptionConfig = new AuthenticatedEncryptorConfiguration()
{
    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
    ValidationAlgorithm = ValidationAlgorithm.HMACSHA512
};

builder.Services.AddDataProtection()
    .ProtectKeysWithDpapiNG()
    .UseCryptographicAlgorithms(encryptionConfig)
    .SetApplicationName("myappname")
    // Blob-Store
    .PersistKeysToAzureBlobStorage(new Uri("{BLOB URI}"), credential)
    // Key-Vault
    .ProtectKeysWithAzureKeyVault(new Uri("{KEY IDENTIFIER}"), credential)
    // SQL Server
    .PersistKeysToDbContext<ApplicationDbContext>()
    .AddKeyManagementOptions(options => options.NewKeyLifetime = TimeSpan.FromDays(90));
```

**Warum**: Eine infizierte Maschine hat zumindest die Schlüssel nicht lokal am Dateisystem.

## 🛡 Exception page

```csharp
if(environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseStatusCodePageWithRedirects("/error/{0}");
}
```

**Warum**: Keine Details bei einem Produktiv oder Test-Deployment.

## ⚙ Middleware Order
![The correct order of the dot net middleware](https://www.push-force.dev/resource/DotnetMiddlewareOrder.svg#center#full)

**Warum**: Die Reihenfolge ist relevant\
**Warum**: Caching vor CORS würde z.B. eine Response liefern, obwohl die origin nicht passen könnte.\
**Warum**: Authentication ist abhängig vom Routing,...

## ⚙ Authentication: Intranet

```csharp
// self-hosted (Only Kestrel, no IIS, no AppService)
builder.UseHttpSys(options =>
{
    // ✅
    options.Authentication.Schemes = AuthenticationSchemes.Kerberos;
    options.Authentication.AllowAnonymous = false;
    // ⚠️ not wrong per default, but could switch to NTLM.
    // NTLM is not that secure and Windows wants to get rid of it.
    options.Authentication.Schemes = AuthenticationSchemes.Negotiate;
});

services.AddAuthentication(HttpSysDefaults.AuthenticationScheme);

// IIS
builder.UseIIS();
services.AddAuthentication(IISDefaults.AuthenticationScheme);
```

**Warum**: Authentication wird von IIS bereitgestellt und wir müssen uns um nichts kümmern. \
**Warum**: Weil Microsoft mehr Geld, Ressourcen und Knowhow besitzt. \
**Warum**: Standardisierter Workflow in unserem Code und somit bessere Wartbarkeit. \
**Warum**: Verantwortung und Wartung wird ausgelagert

## ⚙ Authentication: Internet

Die Verwendung von OpenID ist zu bevorzugen.

**Warum**: Weil andere IdentityProvider mehr Geld, Ressourcen und Knowhow besitzen.\
**Warum**: Standardisierter Workflow in unserem Code und somit bessere Wartbarkeit\
**Warum**: Verantwortung und Wartung wird ausgelagert

# ASP NET MVC

## ⚔️ Redirect Attack

```csharp
// ❌
Redirect(url);
// ✅
if(IsLocalUrl(url))
    Redirect(url)
```

**Warum**: Verhindert, dass die Applikation auf eine andere Applikation umgeleitet wird.

## ⚔️ Directory Traversal Attack

```csharp
// ❌
var req = Request.QueryString["filename"];
var file = new FileInfo(req);
// ✅
var req = Request.QueryString["filename"];
var path = Server.MapPath(Path.Combine("documents", req);
var file = new FileInfo(path);
```

**Warum**: Pfade außerhalb von `wwwroot` werden nicht zurückgegeben.

## ⚔️ XSS: Output-Encoding

Die Verwendung von `[ValidateInput(false)]` sollte vermieden werden. [MvcHtmlString](https://learn.microsoft.com/de-de/dotnet/api/system.web.mvc.mvchtmlstring?view=aspnet-mvc-5.2) sollte verwendet werden, um string encoding zu verhindern.

```csharp
// ❌
[ValidateInput(false)]
public async Task MyEndpoint(Model model) { ...

// ✅
public class Model {
    [AllowHtml]
    public string HtmlContent { get; set; }

// ℹ
var username = "<b>Mark</b>";
@username                  // <b>Mark</b>              (HTML Tags are encoded)
@Html.Encode(username);    // &lt;b&gt;Mark&lt;/b&gt;  (HTML Tags are double encoded)
@Html.Raw(username)        // **Mark**                 (Mark is actually printed bold)
```

**Warum**: HTML-Encoding wird bei allen anderen Properties weiterhin verwendet.

## 🛡 Input validation: ModelState Object

```csharp
public ActionResult Register(RegisterViewModel model)
{
    if(string.IsNullOrEmpty(model.Username))
        ModelState.AddModelError(nameof(model.Username), "Username invalid");

    if(ModelState.IsValid)
        ...
```

```csharp
[Required]
public string Username { get; set; }
[Range]
public int Age { get; set; }
```

**Warum**: Model validation ist bereits Teil des Frameworks.\
**Warum**: Funktioniert auch out-of-the-box mit attribute annotations.\
**Warum**: Eigene `ValidationAttributes` können hinzugefügt werden.\
**Warum**: Mit `return BadRequest(ModelState)`-Methode in API-Controllern kann das Validierungsergebnis bequem an den Client gesendet werden.

# ASP NET Forms

## 🛡 Input validation: Validators

Verwende folgende server-side input-validation controls des Frameworks:

```html
<asp:RequiredFieldValidator />
<asp:RangeValidator />
<asp:CompareValidator />
<asp:RegularExpressionValidator />
<asp:CustomValidator />
```

mit `<asp:ValidationSummary />` kann eine Zusammenfassung ausgegeben werden.

**Warum**: Das Framework übernimmt bereits Sanitization und ein sicheres Encoding.

## ⚔️ ViewState Tampering

Der `ViewState` wird base64 encodiert an der Server gesendet. Tampering durch einen on-path-attack ist somit nicht ausgeschlossen.

```xml
<configuration>
    <system.web>
        <pages 
            validateRequest="true"
            enableViewStateMac="true"
            viewStateEncryptionMode="Always" />
    </system.web>
</configuration>
```

**Warum**: Nicht encodiertes HTML und JS werden automatisch vom Server abgelehnt.\
**Warum**: Der Inhalt ist verschlüsselt\
**Warum**: Es wird eine HMAC an den ViewState angehängt. Wurde der ViewState manipuliert, wird dieser vom Server verworfen.

## ⚔️ Impersonation Attack

Gruppen- oder Benutzerbezogene Daten sollten nicht über die Berechtigung des ApplicationPools bezogen oder manipuliert werden. Es ist möglich automatisch in den Authentifizierten User-Scope zu schlüpfen.

```xml
<configuration>
    <system.web>
        <identity impersonate="true" />
        <!-- It is also possible to map all users to one user -->
        <identity impersonate="true" userName="domain\Tschinness" password="..." />
    </system.web>
</configuration>
```

**Warum**: Im Scope des Applicationpools könnte der Applikationsuser auf Ressourcen (SMB, AD, Dateien,...) zugreifen, obwohl dessen entsprechende Berechtigungen fehlen.\
**Warum**: Alle Zugriffe werden auf die Rechte des Authentifizierten Benutzers geprüft.

## ⚔️ Authentication Bypass Attack

```csharp
// ❌
Server.Transfer(...);
// ✅
Response.Redirect(...);
```
**Warum**: Beim Transfer wird die Request 1 zu 1 weitergeleitet - inklusive der Cookies. Ein redirect sendet zuerst ein HTTP 30X und der Client meldet sich dann selbst bei dem anderen Zielsystem.

## ⚙ Secure Pages structure

Endpunkte die keine Authentifizierung benötigen, sind in einem anderen Unterordner abzulegen. So gibt es keine Vermischung von Seiten die eine Authentifizierung voraussetzen und welche die anonym erreichbar sind.

```xml
<configuration>
    <location path="webApp/Pages/NonSecure">
        <system.web>
            <authorization>
                <!-- allow any anonymous user -->
                <allow users="?" />
            </authorization>
        </system.web>
    </location>
    
    <location path="webApp/Pages/Secure">
        <system.web>
            <authorization>
                <!-- allow any authenticated user -->
                <allow users="*" />
            </authorization>
        </system.web>
    </location>

    <location path="webApp/Pages/Secure/Admin">
        <system.web>
            <authorization>
                <!-- allow only admins -->
                <allow roles="Admin" />
                <deny users="*" />
            </authorization>
        </system.web>
    </location>
</configuration>
```

**Warum**: Klare Trennung der Seiten.\
**Warum**: Authentifizierung wird, aufgrund der Ordnerstruktur, automatisch vorausgesetzt.

## ⚙ Don't override the `machine.config`

```xml
<location path="" allowOverride="false">
    ...
</location>
```

**Warum**: Globale Einstellungen wurden aus einem bestimmten Grund getroffen. Alle Applikationen müssen den selben, globalen Richtlinien folgen.

## ⚙ Debug Mode

```xml
<configuration>
    <system.web>
        <compilation debug="false" />
    </system.web>
</configuration>
```

**Warum**: Debug braucht mehr RAM\
**Warum**: Debug ist langsamer\
**Warum**: Debug hat einen längeren Startup\
**Warum**: `WebResources.axd` kann nicht geändert werden

## ⚙ Don't leave any trace

```xml
<configuration>
    <system.web>
        <trace enabled="false" />
    </system.web>
</configuration>
```

**Warum**: Bei der Ausgabe von Fehlern auf der Weboberfläche könnten zu detailierte Informationen dem Benutzer zurückgegeben werden.

# ASP NET Forms und MVC

## ⚔️ Denial of Service Attack

```xml
<configuration>
    <system.web>
        <httpRuntime 
            maxRequestLength="2000" 
            requestTimeout="8"
            useFullyQualifiedRedirectUrl="true" />
    </system.web>
</configuration>
```

**Warum**: Kleinere payloads erschweren einen DoS.\
**Warum**: Kürzere Auslastung pro Request im Fehlerfall.\
**Warum**: Absolute Pfade lassen keinen Platz für falsche Interpretation.

## ⚙ Authentication: Intranet

Es gibt neben `Windows` noch `Forms`, `None`, `Passport`, und `Federated`.

```xml
<configuration>
    <system.web>
        <authentication mode="Windows" />
    </system.web>
</configuration>
```

**Warum**: Authentication wird von IIS bereitgestellt und wir müssen uns um nichts kümmern.\
**Warum**: Weil Microsoft mehr Geld, Ressourcen und Knowhow besitzt.\
**Warum**: Standardisierter Workflow in unserem Code und somit bessere Wartbarkeit.\
**Warum**: Verantwortung und Wartung wird ausgelagert

## ⚙ Single-Sign On

Jede SSO unterstützende Applikation muss den selben `machineKey` verwenden.

```xml
<configuration>
    <system.web>
        <machineKey validationKey="..." decryptionKey="..." validation="..." decryption="..." />
    </system.web>
</configuration>
```

**Warum**: Funktioniert out-of-the-box

## ⚙ Authorization: Intranet

Routen können auf Rollen eingeschränkt werden. Diese werden automatisch über COM Schnittstellen abgefragt und bestehen aus AD Benutzer und AD Gruppen.

```xml
<configuration>
    <system.web>
        <roleManager enabled="true" cacheRolesInCookie="true" />
    </system.web>
    <location path="membersOnly">
        <system.web>
            <authorization>
                <allow roles="members" />
            </authorization>
        </system.web>
    </location>
</configuration>
```

**Warum**: Funktioniert out-of-the-box

## ⚙ Authentication: Internet

Die Verwendung von OpenID ist zu bevorzugen.

```xml
<configuration>
    <system.web>
        <authentication mode="Federated" />
    </system.web>
</configuration>
```

**Warum**: Weil andere IdentityProvider mehr Geld, Ressourcen, und Knowhow besitzen.\
**Warum**: Standardisierter Workflow in unserem Code und somit bessere Wartbarkeit\
**Warum**: Verantwortung und Wartung wird ausgelagert.

## ⚙ Health Monitoring

```xml
<configuration>
    <system.web>
        <healthMonitoring enabled="true" />
    </system.web>
</configuration>
```

**Warum**: Unübliche Zugriffe, Programmabläufe, und Erreichbarkeitsschwierigkeiten werden protokolliert und tragen zur Fehlerfindung und -behebung bei.

## 🛡 Authentication: Hardening

```xml
<form runAt="server">
    <asp:Login runAt="server" displayRememberMe="false" />
</form>
```

**Warum**: User-Credentials werden nicht im Cookie gespeichert und können somit bei Übernahme der Maschine nicht gestohlen werden.

```xml
<configuration>
    <system.web>
        <authentication mode="..." protection="All" requireSSL="True">
            <credentials passwordFormat="SHA1">
                <user name="Mark" password="..." />
            </credentials>
            <forms cookieLess="UseCookies"
                slidingExpiration="false"
                name="myApp"
                path="myApp/" />
        </authentication>
    </system.web>
</configuration>
```

**Warum**: `Protection` Verschlüsselt das Auth-Ticket und garantiert dessen Integrität.\
**Warum**: `PasswordFormat` stellt sicher, dass bei der Übernahme der Maschine keine Passwörter im Klartext zur Verfügung stehen.\
**Warum**: `UseCookies` überträgt das Auth-Ticket als Cookie, anstatt es in der URL zu übertragen.\
**Warum**: `SlidingExpiration` verlängert die Gültigkeit des Auth-Tickets bei jeder Anfrage.\
**Warum**: `Name` schränkt die Gültigkeit des Auth-Tickets auf die genannte Applikation ein.\
**Warum**: Werden mehrere Applikationen auf dem Server betrieben, so kann der Gültigkeitsbereich des Auth-Tickets mit `path` eingeschränkt werden.

```xml
<configuration>
    <system.web>
        <machineKey
            encryptionKey="AutoGenerate.IsolateApps"
            decryptionKey="AutoGenerate.IsolateApps"
            decryption="AES"
            validation="HMACSHA512"
            protection="All" />
    </system.web>
</configuration>
```

**Warum**: Sichere Algorithmen werden verwendet.\
**Warum**: Das Auth-Ticket bei SSO auf Integrität geprüft.

```xml
<configuration>
    <system.web>
        <membership defaultProvider="SqlProvider">
            <providers>
                <clear />
                <add name="SqlProvider" connectionString="..." applicationName="..."
                    passwordAttemptWindow="30"
                    maxInvalidPasswordAttempt="3"
                    passwordStrengthRegularExpresion="..."
                    minRequiredNonAlphanumericCharacters="4"
                    minRequiredPasswordLength="12"
                    enablePasswordRetrieval="False"
                    requiresUniqueEmail="True"
                    passwordFormat="Hashed"
            </providers>
        </membership>
    </system.web>
</configuration>
```
**Warum**: Vernünftige Einstellungen um den User vor Brute Force Angriffen zu schützen.

```csharp
// ❌
FormsAuthentication.RedirectFromLoginPage(username, true);
FormsAuthentication.SetAuthCookie(username, true);
FormsAuthentication.GetRedirectUrl(username, true);
var ticket = new FormsAuthenticationTicket(username, true, ...);
// ✅
FormsAuthentication.RedirectFromLoginPage(username, false);
FormsAuthentication.SetAuthCookie(username, false);
FormsAuthentication.GetRedirectUrl(username, false);
var ticket = new FormsAuthenticationTicket(username, false, ...);
```
**Warum**: Cookie wird nicht am Client persistiert und ist somit auch nicht zugänglich falls der Client infiltriert wurde.

## 🛡 Session: Hardening

```xml
<configuration>
    <system.web>
        <roleManager 
            cookieRequireSSL="true"
            cookieProtection="All"
            cookieTimeout="10"
            createPersistentCookie="false" />
        <sessionState timeout="10"
            regenerateExpiredSessionId="true"
            cookieless="UseCookies"
            encrypt="true" />
    </system.web>
</configuration>
```

```csharp
public static void ResetSession()
{
    HttpContext.Current.Session.Clear();
    HttpContext.Current.Session.Abandon();
    HttpContext.Current.Session.RemoveAll();
    HttpContext.Current.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", "");
    HttpContext.Current.response.Cookies["ASP.NET_SessionId").Expire = DateTime.Now.AddMonths(-20);
}
```

**Warum**: Ein Angreifer hat weniger Zeit eine Session zu klauen, weil diese regelmäßig erneuert wird.\
**Warum**: Die Session wird beim Log-Out auf dem Server und Client terminiert.

## 🛡 Cookie: Hardening

```xml
<configuration>
    <system.web>
        <httpCookies 
            httpOnlyCookies="true"
            requireSSL="true" />
    </system.web>
</configuration>
```

**Warum**: Nur der Browser darf auf die Cookies zugreifen, kein böses Javascript.\
**Warum**: Das übertragene Cookie kann durch einen man-in-the-middle attack nicht ausgelesen werden.

## 🛡 HttpHandler: Hardening

Wie und welche Dateien die Webapplikation verarbeitet kann über HTTP-Handler konfiguriert werden. Nicht benötigte Dateiformate müssen ausgenommen werden.

```xml
<configuration>
    <system.web>
        <httpHandlers> 
            <add verb="*" path="*.xml" type="System.Web.HttpForbiddenHandler" /> 
        </httpHandlers>
    </system.web>
</configuration>
```

**Warum**: Hindert die Webapplikation daran diverse Dateien zu verarbeiten bzw. zu interpretieren.

# IIS

## 🛡 Disable Directory Traveral

Um bei Funktionsaufrufen wie `MapPath` kein directory traveral zuzulassen, kann folgende Einstellung gesetzt werden: App → Behaviour → Enable Parent Paths auf `false`. 

## 🛡 Request Filtering

Der IIS kann out-of-the-box bereits eingehenden Traffic ablehnen. Einstellungen dazu können im Unterpunkt "Request Filtering" vorgenommen werden. Falls nicht verfügbar:

- Windows Server: Add feature → IIS → Web Server → Request filtering
- Windows: Turn on/off features → Security → Request filtering

Neben black- und whitelists für Dateien, HTTP headers, content-length restrictions usw. kann auch die Request auf verdächtige Inhalte überprüft werden:

- `allowDoubleEscaping=false`: Doppeltes Escaping wird nicht akzeptiert
- `allowHighBitCharacters=true`: Non-ASCII Werte werden akzeptiert
- `unescapeQueryString=true`: Decodiert die Anfrage solange, bis keine Änderung beim dekodieren mehr festzustellen ist
- `removeServerHeader=true`: Verbirgt den IIS
- `alwaysAllowQueryStrings=false`: Query Parameter werden ebenso auf deren Inhalt überprüft. E.g. `&path=../secrets.txt` würde der IIS blockieren

## 🛡 Website Location

Die Website sollte auf keinen Fall auf dem Standardpfad  `C:/inetpub/wwwroot` bereitgestellt werden. Denn auf dem `C:` Laufwerk ist in der Regel die Windows Installation und mithilfe eines Directory Traversal Attacks könnte auf alles (z.B. cmd.exe) zugegriffen werden. Empfehlung: Eigene Partition mit Unterordnern kombiniert mit ACLs pro Application-Pool. Diese Einstellung betrifft auch die Logs, welche im Unterpunkt `Logging` angepasst werden kann.

## 🛡 Handler Mappings

Der IIS entscheidet, jenachdem welche Datei angefordert wird, was damit passieren soll. E.g. eine PHP Datei würde vom IIS interpretiert und das Ergebnis an den Client ausgeliefert werden. Somit sollte unter `Handler Mappings` alles deaktiviert werden was nicht gebraucht wird.

## 🛡 Anonymous User

Unter `Authentication` kann der anonymous user aktiviert werden, um die Webapp ohne Benutzerkonto ansteuern zu können. Für jede Website sollte ein eigener anonymous user verwendet werden, um seinen Wirkungsbereich nicht auf andere Webapplikationen auszuweiten. Hiefür den anonymous user auswählen → Edit → select identity.

## 🛡 Permissions

Die Zugriffe der verwendeten Benutzer (Logging, anonymous, app-pool) muss über die ACL eingeschränkt werden:

- Read: Nur bei Ordnern die ausgeliefert werden können
- Execute: Nur der app-pool und nur auf dem Pfad der Applikation
- Write: Nur bei Ordnern die dafür vorhergesehen wurden

## 🛡 IP Restriction

Sollte die Webapplikation nur von einer bestimmten IP erreichbar sein, kann diese im Unterpunkt `IP Address and Domains` mithilfe einer Whitelist eingestellt werden.

## 🛡 Remove Extensions

IIS liefert div. Erweiterungen mit, welche, sofern nicht benötigt, entfernt werden müssen. Unter `ISAPI Filters` sind alle aktivierten Erweiterungen zu finden. Ein Beispiel: **fpexedll** fügt die Funktionalität von Microsoft FrontPages hinzu.

## ⚙ Authentication

`Integrated Windows Authentication` > `Digest Authentication`\
⚠ Konfiguriere den IIS auf `Kerberos`, da NTLM andere Sicherheitsbedenken mit sich bringt.

# SQL-Server

## ⚔️ SQL-Injection

Ohne zwischengeschaltete Softwarelösung ist der SQL-Server ein einfaches Ziel für SQL-Injection-Angriffe. Jeder Freitextparameter, in Kombination mit `EXECUTE`, `EXEC`, oder `sp_executesql`, kann mit böswilligen Inhalten befüllt werden.

```sql
// ❌
EXEC sp_executesql 'TRUNCATE TABLE ' + @TableName;
// ✅
// TODO: Does the table exist?
// TODO: Should we restrict the truncatable tables?
EXEC sp_executesql 'TRUNCATE TABLE ' + QUOTENAME(@TableName);
```

**Warum**: `QUOTENAME` setzt den Freitextparameter unter Anführungszeichen, sodass dieser auch korrekt als String interpretiert wird.

```sql
// ❌
EXEC sp_executesql 'SELECT * FROM table WHERE column = ' + @value;
// ✅
EXEC sp_executesql N'SELECT * FROM table where column = @value', N'@value varchar(128)', @value;
```

**Warum**: `@value` ist ein gebundener Parameter und wird nicht als SQL interpretiert.

```sql
// ❌
DECLARE @Blacklist TABLE (Token NVARCHAR(200) NOT NULL);
INSERT INTO @Blacklist (Token) values (';'), ('--'), ('DROP'), ('EXEC'), ('/*'),...

IF EXISTS (SELECT 1 FROM @Blacklist b WHERE CHARINDEX(b.Token, UPPER(@UserInput)) > 0)
BEGIN
    // @UserInput beinhaltet einen unzulässigen Token
END
// ✅
DECLARE @Whitelist TABLE (Token NVARCHAR(200) NOT NULL);
DECLARE @Userlist TABLE (Token NVARCHAR(200) NOT NULL);

INSERT INTO @Whitelist (Token) values ('SELECT'), ('TABLE1'), ('TABLE2'), ('TABLE3'), ('WHERE'),...
INSERT INTO @Userlist (Token) SELECT value FROM STRING_SPLIT(UPPER(@UserInput), ' ') WHERE value <> '';

IF EXISTS (SELECT 1 FROM @Userlist u LEFT JOIN @Whitelist w ON u.Token = w.Token WHERE w.Token IS NULL)
BEGIN
    // @UserInput beinhaltet einen unzulässigen Token
END
```

**Warum**: Wir geben die möglichen Inhalte vor. Alles, was nicht definiert wurde, wird automatisch abgelehnt.\
**Warum**: Angreifer sind sehr kreativ. Eine Blacklist ist nie vollständig.

## ⚙ Authentication

- Deaktiviere den SA-Account
- Wird der SA-Account dennoch benötigt, benenne ihn um
- Konfiguriere als Datenbank-Owner ein SQL-Account. Anschließend: Security → Logins → Properties → Permission to connect auf `deny` und Permission to login auf `disable`
- Verwende ausschließlich Active-Directory oder Windows-Authentication
- Entferne alle lokalen Administratoren (falls vorhanden)

```sql
ALTER LOGIN sa WITH NAME = 'CuSqlServerAdminUser'
```

**Warum**: Der SA-User hat zu viele Rechte und ist allgemein bekannt.\
**Warum**: Die Rechte eines DB-Owners werden so gut wie nie benötigt.\
**Warum**: Entra-ID-Benutzer und -Gruppen können bei einer Migration einfach übernommen werden.\
**Warum**: Lokale Server-Administratoren haben in der Datenbank nichts zu suchen.

## ⚙ Password Policies

Wird der mixed-mode benötigt, verwende eine vernünftige Password policy. Dazu: Start → Control Panel → Administrative Tools → Local Security Policy. Im SQL-Server: Benutzer auswählen → Security → Enforce password policy

## ⚙ Permission Management

- Erstelle eine Entra-ID-Gruppe und füge die benötigten Benutzer hinzu.
- Erstelle auf der Datenbank eine neue Rolle mit den minimal notwendigen Berechtigungen. (Least Privilege)
- Teile die Rolle der Entra-ID-Gruppe zu
- Service-User besitzen die `SysAdmin` Rolle

```sql
EXEC sp_addrolemember N'db_datareader', N'CUBIDO\ProjectXYZDBReadOnly'
```

**Warum**: Benutzer können schnell hinzugefügt oder entfernt werden.\
**Warum**: Es entsteht kein Wildwuchs an Berechtigungen.\
**Warum**: Infrastruktur ist oft schneller greifbar als ein DB-Security-Admin.

## ⚙ Certificate

Hinterlege ein gültiges und in der Domain anerkanntes Zertifikat: SQL Server Manager (sqlservermanagerXX.msc) → SQL Server Network Configuration → Protocols → Properties → Certificate → Browse

**Warum**: Wird keines hinterlegt, generiert der SQL-Server ein self-signed-certificate. Dieses Zertifikat kann keine Aussage über die Echtheit der Identität des SQL-Servers tätigen.\
**Warum**: Mit `TrustServerCertificate=True` vertrauen wir blind und sind Impersonation Attacks ausgeliefert.

## ⚙ Enable Auditing

Login Fehlversuche sollten protokolliert werden: Server Management Studio → server-instance → properties → security → login auditing auf `Failed logins only` stellen.

**Warum**: Erfolgreiche Logins überfüllen die Logs - wir sind nur an Fehlversuchen interessiert.

## 🛡 Hide the SQL-Server

- Task-Manager → Services → SQL Server Browser → Eigenschaften → Startup type auf `disabled`
- SQL Server Manager (sqlservermanagerXX.msc) → SQL Server Network Configuration → Protocols → Properties → Hide Instance auf `true`
- SQL Server Manager (sqlservermanagerXX.msc) → SQL Server Network Configuration → Protocols → Properties → TCP Dynamic Port die `0` entfernen und den [TCP Port](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/configure-a-server-to-listen-on-a-specific-tcp-port?view=sql-server-ver17#assign-a-tcpip-port-number-to-the-sql-server-database-engine) beliebig setzen

**Warum**: Wir wissen, wie der SQL-Server erreichbar ist – keinen Grund, diesen auffindbar zu machen.\
**Warum**: Port-Scanner sind laut, und IDS sind darauf ausgelegt, diese zu erkennen und zu isolieren.\
**Warum**: Ohne SQL Server Browser muss ein statischer Port vergeben werden, da bei jedem Service-Restart sonst ein neuer beliebiger Port vergeben wird.

## 🛡 Disable Features

- Im SQL Server Configuration Manager können Dienste, welche nicht benötigt werden, deaktiviert werden. 
- Jeder Dienst sollte sein eigenes Service-Account mit eingeschränkten Rechten besitzen.
- Sicherstellen das `xp_cmdshell` deaktiviert ist.
- Sicherstellen das `cross db ownership chaining` deaktiviert ist.

```sql
EXEC sp_configure 'show advanced options', 1;
GO
RECONFIGURE
GO
EXEC sp_configure 'xp_cmdshell', 0;
EXEC sp_configure 'cross db ownership chaining', 0;
GO
RECONFIGURE
GO
```

⚠️ Wird `cross db ownership chaining` dennoch benötigt, nur auf der entsprechenden Datenbank aktivieren.

```sql
ALTER DATABASE <DB> SET DB_CHAINING ON; 
```

**Warum**: Angriffsfläche minimieren.\
**Warum**: `xp_cmdshell` hat die Rechte des SQL-Server-Dienstkontos. Es handelt sich um eine Stored Procedure, welche es dem Benutzer erlaubt, Shell-Commands abzusetzen.\
**Warum**: `cross db ownership chaining` ignoriert die Berechtigungen bei datenbankübergreifenden Abfragen, sofern der Eigentümer der Objekte derselbe ist.

## 🛡 Use Encryption

Entscheide nach eigenem Ermessen, welche Teile des Systems verschlüsselt werden müssen. Weiteres kann die Entschlüsselung von Daten auf [Benutzergruppen](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-symmetric-key-transact-sql?view=sql-server-ver17&utm_source=chatgpt.com) eingeschränkt werden, sodass etwaige Dienste zwar prinzipiell eine Leseberechtigung besitzen, aber keine Einsicht auf verschlüsselte Spalten erhalten.

- Unverzichtbar: [Sichere Verbindungen](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/configure-sql-server-encryption?view=sql-server-ver17)
- Empfohlen: [Ganze Datenbank](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/transparent-data-encryption?view=sql-server-ver17#enable-tde)
- Möglich: [Sensitive Spalten](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/encrypt-a-column-of-data?view=sql-server-ver17&utm_source=chatgpt.com)
- Möglich: [Kritische Prozeduren](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-procedure-transact-sql?view=sql-server-ver17&utm_source=chatgpt.com)

SQL Server Manager (sqlservermanagerXX.msc) → SQL Server Network Configuration → Protocols → Properties → Force encryption auf `true`

⚠️ Die Datenbankverschlüsselung sollte nur dann verwendet werden, wenn das Einspielen eines Backups entsprechend dokumentiert und testweise durchgeführt wurde. Eine verschlüsselte Datenbankwiederherstellung ist nicht trivial und kann die Kompetenz von Server-Administratoren übersteigen.