# dtmcli.efcore

https://dtm.pub
DTM��һ�Դ�ķֲ�ʽ�������������������ݿ⡢����񡢿�����ջ�������ݵ�һ�������⡣

https://github.com/dtm-labs/dtmcli-csharp
dtmcli-csharp �Ƿֲ�ʽ��������� DTM �� C# �ͻ��ˣ�ʹ�� HTTP Э��� DTM ����˽��н�����

dtmcli.efcore �� dtmcli-csharp �����Ͻ����˼򵥷�װ, ��չ�����¹��ܵ㣺
- ��efcore��֧��
- ���������ϱ���Զ�����
- QueryPrepared�ز�ӿڵ�Ĭ��ʵ��
- �����ύ�����������ϵĴ����

�� dtmcli-csharp ԭ��ʹ�÷�����Ӱ�졣

### ����
```json
{
  "dtm": {
    "DtmUrl": "http://localhost:36789",
    "DtmTimeout": 10000,
    "BranchTimeout": 10000,
    "DBType": "mysql",
    "BarrierTableName": "barrier",

    //��չ��
    "HostName": "localhost:5000", //�������ǰ����host�Ͷ˿�
    "QueryPreparedPath": "/dtm/queryprepared"; //��ѡ�Ĭ��Ϊ/dtm/queryprepared
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

### �÷�

```c#
//������������

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
        .EnableWaitResult(); //�ȴ���������

    await fullMsg.DoAndSubmitDB(async () =>  //����������Ϣ�� + ����ҵ��ű���
    {
        await _dbContext.SaveChangesAsync();
    });
}

```

```c#
//������
[HttpPost("barrierTransOut")]
public async Task<IActionResult> BarrierTransOut(
    [FromBody] TransRequest body,
    [FromServices] IFullBranchBarrierFactory fullBranchBarrierFactory)
{
    _logger.LogInformation("barrierTransOut, QueryString={0}", Request.QueryString);

    var branchBarrier = fullBranchBarrierFactory.CreateBranchBarrier(HttpContext.Request.Query)!;
    await branchBarrier.Barrier(async () =>
    {
        _logger?.LogInformation("�û�: {0},ת�� {1} Ԫ---�ع�", body.UserId, body.Amount);
        await _myService.DoWork();
    });
    return Ok();
}
```