using ScaffoldGenerator.Contracts.Packages;

namespace ScaffoldGenerator.Application.Packages;

/// <summary>
/// 常用包预置列表
/// </summary>
public static class PopularPackages
{
    /// <summary>
    /// NuGet 常用包
    /// </summary>
    public static readonly IReadOnlyList<PackageInfo> NuGet =
    [
        new("Serilog.AspNetCore", "9.0.0", "日志框架"),
        new("AutoMapper", "12.0.1", "对象映射"),
        new("FluentValidation", "11.9.0", "验证框架"),
        new("MediatR", "12.2.0", "中介者模式"),
        new("Polly", "8.2.0", "弹性策略"),
        new("Mapster", "7.4.0", "高性能对象映射"),
        new("Dapper", "2.1.35", "轻量级 ORM"),
        new("Newtonsoft.Json", "13.0.3", "JSON 序列化"),
        new("StackExchange.Redis", "2.7.33", "Redis 客户端"),
        new("Quartz", "3.8.1", "任务调度")
    ];

    /// <summary>
    /// npm 常用包
    /// </summary>
    public static readonly IReadOnlyList<PackageInfo> Npm =
    [
        new("axios", "1.6.2", "HTTP 客户端"),
        new("dayjs", "1.11.10", "日期处理"),
        new("lodash-es", "4.17.21", "工具库"),
        new("@vueuse/core", "10.7.0", "Vue 组合式工具"),
        new("pinia", "2.1.7", "Vue 状态管理"),
        new("vue-router", "4.2.5", "Vue 路由"),
        new("@tanstack/vue-query", "5.17.0", "数据获取"),
        new("zod", "3.22.4", "Schema 验证"),
        new("nanoid", "5.0.4", "ID 生成"),
        new("mitt", "3.0.1", "事件总线")
    ];
}
