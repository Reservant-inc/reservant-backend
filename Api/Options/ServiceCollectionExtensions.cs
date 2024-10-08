namespace Reservant.Api.Options;

/// <summary>
/// Extension methods to register configuration-related services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register configuration options
    /// </summary>
    public static void AddConfigurationOptions(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.ConfigSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FileUploadsOptions>()
            .BindConfiguration(FileUploadsOptions.ConfigSection)
            .ValidateDataAnnotations()
            .Validate(
                o => Path.EndsInDirectorySeparator(o.GetFullSavePath()),
                $"{nameof(FileUploadsOptions.SavePath)} must end with /")
            .Validate(
                o => !Path.EndsInDirectorySeparator(o.ServePath),
                $"{nameof(FileUploadsOptions.ServePath)} must not end with /")
            .Validate(
                o => !Path.EndsInDirectorySeparator(o.ServeUrlBase),
                $"{nameof(FileUploadsOptions.ServeUrlBase)} must not end with /")
            .ValidateOnStart();

        services.AddOptions<FirebaseOptions>()
            .BindConfiguration(FirebaseOptions.ConfigSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
