using FluentAssertions;
using ScaffoldGenerator.Application.Abstractions;
using ScaffoldGenerator.Application.Modules;
using ScaffoldGenerator.Application.Providers;
using ScaffoldGenerator.Contracts.Enums;
using ScaffoldGenerator.Contracts.Requests;
using Xunit;

namespace ScaffoldGenerator.Tests.Modules;

public class FrontendModuleTests
{
    private static readonly IUiLibraryProvider[] Providers =
    [
        new ElementPlusProvider(),
        new AntDesignProvider(),
        new NaiveUiProvider(),
        new TailwindProvider(),
        new ShadcnVueProvider(),
        new MateChatProvider()
    ];

    private readonly FrontendModule _module = new(Providers);

    [Fact]
    public void Name_ReturnsFrontend()
    {
        _module.Name.Should().Be("Frontend");
    }

    [Fact]
    public void Order_ShouldBe100()
    {
        _module.Order.Should().Be(100);
    }

    [Fact]
    public void IsEnabled_Always_ReturnsTrue()
    {
        var request = CreateRequest();
        _module.IsEnabled(request).Should().BeTrue();
    }

    [Fact]
    public async Task ContributeAsync_ElementPlus_AddsElementPlusPackages()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(UiLibrary.ElementPlus);

        await _module.ContributeAsync(plan, request, default);

        plan.NpmPackages.Should().Contain(p => p.Name == "element-plus");
    }

    [Fact]
    public async Task ContributeAsync_AntDesignVue_AddsAntDesignPackages()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(UiLibrary.AntDesignVue);

        await _module.ContributeAsync(plan, request, default);

        plan.NpmPackages.Should().Contain(p => p.Name == "ant-design-vue");
    }

    [Fact]
    public async Task ContributeAsync_NaiveUI_AddsNaivePackages()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(UiLibrary.NaiveUI);

        await _module.ContributeAsync(plan, request, default);

        plan.NpmPackages.Should().Contain(p => p.Name == "naive-ui");
    }

    [Fact]
    public async Task ContributeAsync_TailwindHeadless_AddsTailwindPackages()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(UiLibrary.TailwindHeadless);

        await _module.ContributeAsync(plan, request, default);

        plan.NpmPackages.Should().Contain(p => p.Name == "tailwindcss");
        plan.NpmPackages.Should().Contain(p => p.Name == "@headlessui/vue");
        plan.Files.Should().Contain(f => f.OutputPath.Contains("tailwind.config.js"));
    }

    [Fact]
    public async Task ContributeAsync_ShadcnVue_AddsShadcnPackages()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(UiLibrary.ShadcnVue);

        await _module.ContributeAsync(plan, request, default);

        plan.NpmPackages.Should().Contain(p => p.Name == "radix-vue");
        plan.NpmPackages.Should().Contain(p => p.Name == "tailwindcss");
        plan.Files.Should().Contain(f => f.OutputPath.Contains("components.json"));
    }

    [Fact]
    public async Task ContributeAsync_MateChat_AddsMateChatPackages()
    {
        var plan = new ScaffoldPlan();
        var request = CreateRequest(UiLibrary.MateChat);

        await _module.ContributeAsync(plan, request, default);

        plan.NpmPackages.Should().Contain(p => p.Name == "@matechat/core");
        plan.NpmPackages.Should().Contain(p => p.Name == "vue-devui");
        plan.Files.Should().Contain(f => f.OutputPath.Contains("ChatLayout.vue"));
    }

    [Fact]
    public async Task ContributeAsync_AllLibraries_AlwaysAddCorePackages()
    {
        foreach (var library in Enum.GetValues<UiLibrary>())
        {
            var plan = new ScaffoldPlan();
            var request = CreateRequest(library);

            await _module.ContributeAsync(plan, request, default);

            plan.NpmPackages.Should().Contain(p => p.Name == "vue", $"missing vue for {library}");
            plan.NpmPackages.Should().Contain(p => p.Name == "vue-router", $"missing vue-router for {library}");
            plan.NpmPackages.Should().Contain(p => p.Name == "pinia", $"missing pinia for {library}");
            plan.NpmPackages.Should().Contain(p => p.Name == "axios", $"missing axios for {library}");
        }
    }

    private static GenerateScaffoldRequest CreateRequest(
        UiLibrary uiLibrary = UiLibrary.ElementPlus) => new()
    {
        Basic = new BasicOptions
        {
            ProjectName = "TestProject",
            Namespace = "TestProject"
        },
        Frontend = new FrontendOptions
        {
            UiLibrary = uiLibrary
        }
    };
}
