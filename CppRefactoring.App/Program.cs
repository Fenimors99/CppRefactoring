using Avalonia;

AppBuilder.Configure<CppRefactoring.App.App>()
          .UsePlatformDetect()
          .StartWithClassicDesktopLifetime(args);
