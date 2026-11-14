namespace AnimeFeedManager.Features.User.Authentication.RegistrationProcess;

public union RegistrationError(ValidationError, StorageError, AuthError, DeliveryError);

public union AddCredentialError(ValidationError, StorageError, AuthError);
