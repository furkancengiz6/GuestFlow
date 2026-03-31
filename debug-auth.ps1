$headers = @{ "Content-Type" = "application/json"; "X-Tenant-ID" = "1" }
$loginUrl = "http://localhost:5000/api/v1.0/Auth/login"
$body = @{ email = "demo.admin.demo.admin@guestflow.local"; password = "GuestFlow123!" } | ConvertTo-Json

Write-Host "Attempting Login for Demo Admin..."
try {
    $response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -Headers $headers
    Write-Host "Login Successful!"
    Write-Host "Token received (truncated): $($response.accessToken.Substring(0, 20))..."
}
catch {
    Write-Host "Login Failed: $_"
    try {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $respBody = $reader.ReadToEnd()
        Write-Host "Login Error Body: $respBody"
    }
    catch {}
}
