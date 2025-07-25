namespace BasketSurvay.Errors
{
    public record UserErrors
    {
        public static readonly Error UserNotFound =
        new("User.UserNotFound", "can't find User with given Details", StatusCodes.Status404NotFound);

        public static readonly Error InvalidCredintials =
            new("User.InvalidCredintials", "Invalid Email / password", StatusCodes.Status401Unauthorized);

        public static readonly Error DisabledUser =
            new("User.DisabledUser", "Disabled user, please contact your administrator", StatusCodes.Status401Unauthorized);

        public static readonly Error LockedOutUser =
            new("User.LockedOutUser", "wait 5 minuites until your account be unlocked", StatusCodes.Status401Unauthorized);

        public static readonly Error InvalidRefreshCredintials =
            new("User.InvalidRefreshCredintials"
                , "Invalid Token can't generate a refresh token to it",
                StatusCodes.Status401Unauthorized);

        public static readonly Error DuplicatedEmail =
            new("User.DuplicatedEmail", "there are already a signed in user", StatusCodes.Status409Conflict);

        public static readonly Error DuplicatedUsername =
            new("User.DuplicatedUsername", "there are already a Username takes this name ", StatusCodes.Status409Conflict);

        public static readonly Error EmailNotConfirmed =
            new("User.EmailNotConfirmed", "please confirm your email befor trying to signin", StatusCodes.Status401Unauthorized);

        public static readonly Error InvalidCode =
            new("User.InvalidCode", "Invalid verification Code", StatusCodes.Status401Unauthorized);

        public static readonly Error DuplicatedConfirmation =
            new("User.DuplicatedConfirmation", "your email is already confirmed", StatusCodes.Status400BadRequest);

        public static readonly Error InvalidRoles =
        new("User.InvalidRoles", "these Roles is out of the boundary", StatusCodes.Status400BadRequest);

    }
}
