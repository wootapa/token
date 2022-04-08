# token - prints accesstoken (OAuth Client Credentials Flow)

```shell
docker run --rm wootapa/token <token_url> <client_id> <secret> [options...]
```

Options:  
`--decode` is optional and also prints the decoded payload  
`--decodeonly` is optional and only prints the decoded payload

![](usage.gif)