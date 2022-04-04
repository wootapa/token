# token - prints accesstoken (OAuth Client Credentials Flow)

```Shell
docker run --rm wootapa/token ${TOKENURI} ${CLIENTID} ${SECRET} --decode
```

`--decode` is optional and prints the decoded payload

![](usage.gif)