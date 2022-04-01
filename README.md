# token - prints accesstoken (OAuth Client Credentials Flow)

```Shell
docker run --rm wootapa/token "<token_url>" "<client_id>" "<secret>" --decode
```

`--decode` is optional and prints the decoded payload