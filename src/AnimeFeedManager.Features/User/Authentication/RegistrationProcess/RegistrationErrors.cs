namespace AnimeFeedManager.Features.User.Authentication.RegistrationProcess;

[Widens]
public partial union RegistrationError(ValidationError, StorageError, AuthError, DeliveryError);

[Widens]
public partial union AddCredentialError(ValidationError, StorageError, AuthError);
