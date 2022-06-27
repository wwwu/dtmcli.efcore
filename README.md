# dtmcli.efcore

https://dtm.pub
DTM是一款开源的分布式事务管理器，解决跨数据库、跨服务、跨语言栈更新数据的一致性问题。

https://github.com/dtm-labs/dtmcli-csharp
dtmcli-csharp 是分布式事务管理器 DTM 的 C# 客户端，使用 HTTP 协议和 DTM 服务端进行交互。

dtmcli.efcore 在 dtmcli-csharp 基础上进行了简单封装, 扩展了以下功能点：
- 对efcore的支持
- 子事务屏障表的自动创建
- QueryPrepared回查接口的默认实现
- 事务提交及子事务屏障的代码简化

对 dtmcli-csharp 原有使用方法不影响。

### 配置
```json
{
  "dtm": {
    "DtmUrl": "http://localhost:36789",
    "DtmTimeout": 10000,
    "BranchTimeout": 10000,
    "DBType": "mysql",
    "BarrierTableName": "barrier",

    //扩展项
    "HostName": "localhost:5000", //必填项，当前服务host和端口
    "QueryPreparedPath": "/dtm/queryprepared"; //可选项，默认为/dtm/queryprepared
  }
}
```

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        //...

		services.AddDtmcli<MyDbContext>(configuration);
        //services.AddDtmcli<MyDbContext>(x =>
        //{
        //    x.DtmUrl = "http://localhost:36789";    
        //    x.DtmTimeout = 10000; 
        //    x.BranchTimeout = 10000;
        //    x.DBType = "mysql";
        //    x.BarrierTableName = "barrier";
        //
        //    x.HostName = "localhost:5000";
        //});
    }

    public void Configure(IApplicationBuilder app)
    {
        //...

		app.DtmQueryPreparedRegister();
    }
}
```

### 用法

```c#
//主程序、事务发起方

[HttpPost("transfer")]
public async Task<IActionResult> Transfer(
    [FromServices] MyDbContext dbContext,
    [FromServices] IFullDtmTransFactory fullDtmTransFactory)
{
    var account = await dbContext.Accounts.FindAsync(1);
    account.Balance -= 100;

    //Dtm msg
    var fullMsg = fullDtmTransFactory.NewFullMsg();
    var postData = new TransRequest
    {
        UserId = 2,
        Amount = 100,
    };
    fullMsg.Add("http://localhost:5000/barrierTransOut", postData)
        .EnableWaitResult(); //等待子事务结果

    await fullMsg.DoAndSubmitDB(async () =>  //本地事务【消息表 + 本地业务脚本】
    {
        await _dbContext.SaveChangesAsync();
    });
}

```

```c#
//子事务
[HttpPost("barrierTransOut")]
public async Task<IActionResult> BarrierTransOut(
    [FromBody] TransRequest body,
    [FromServices] IFullBranchBarrierFactory fullBranchBarrierFactory)
{
    _logger.LogInformation("barrierTransOut, QueryString={0}", Request.QueryString);

    var branchBarrier = fullBranchBarrierFactory.CreateBranchBarrier(HttpContext.Request.Query)!;
    await branchBarrier.Barrier(async () =>
    {
        _logger?.LogInformation("用户: {0},转出 {1} 元---回滚", body.UserId, body.Amount);
        await _myService.DoWork();
    });
    return Ok();
}
```