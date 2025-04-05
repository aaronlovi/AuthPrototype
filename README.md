# AuthPrototype

This project demonstrates Google Sign-In integration in a Flutter application. It allows users to authenticate using their Google accounts and retrieve basic profile information such as name, email, and profile picture.

## Features
- Google Sign-In authentication
- Display user profile information
- Sign-out functionality

---

## Prerequisites

Before running this project, ensure you have the following:
1. Flutter installed on your system.
2. An Android emulator or physical device with Google Play Services.
3. A Google Cloud Console project configured for Google Sign-In.

---

## Steps to Configure Google Cloud Console

1. **Create a New Project**:
   - Go to the [Google Cloud Console](https://console.cloud.google.com/).
   - Create a new project or select an existing one.

2. **Create an OAuth 2.0 Client ID**:
   - Go to **APIs & Services > Credentials**.
   - Click **Create Credentials > OAuth 2.0 Client ID**.
   - Select **Application type: Android**.
   - Enter your app's **package name** (from `build.gradle.kts`).
   - Add the **SHA-1 fingerprint** of your signing certificate (debug or release).

3. **Add the SHA-1 Fingerprint**:
   - Generate the SHA-1 fingerprint for your keystore:
     ```bash
     keytool -list -v -keystore %USERPROFILE%\.android\debug.keystore -alias androiddebugkey -storepass android -keypass android
     ```
   - Copy the SHA-1 fingerprint and add it to the OAuth 2.0 Client ID configuration.

---

## Matching App Configuration with Google Cloud Console

### 1. **Package Name**
   - The `applicationId` in `android/app/build.gradle.kts` must match the package name in the Google Cloud Console:
     ```kotlin
     defaultConfig {
         applicationId = "com.example.auth_prototype"
     }
     ```

### 2. **SHA-1 Fingerprint**
   - The SHA-1 fingerprint of your signing certificate (debug or release) must be added to the Google Cloud Console.

### 3. **OAuth Client ID**
   - The `meta-data` tag in `AndroidManifest.xml` must match the OAuth Client ID from the Google Cloud Console:
     ```xml
     <meta-data
         android:name="com.google.android.gms.auth.api.signin.v2.com.googleusercontent.apps.<reversed-client-id>"
         android:value="<client-id>.apps.googleusercontent.com" />
     ```

---

## Running the Project

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd AuthPrototype
   ```

2. Install dependences:
   ```flutter pub get```

3. Run the app on an emulator or device:
   ```flutter run```

---

## Troubleshooting

### Error: `ApiException: 10`

- Ensure the **package name** and **SHA-1 fingerprint** in the Google Cloud Console match your app's configuration.
- This is an indication that your project setup does not match the configuration in the Google Cloud Console

### Debugging Tips

- Check the logs for detailed error messages.
- Ensure Google Play Services is up-to-date on your emulator or device.

### Error: NDK Version Mismatch

If you encounter an error like: Your project is configured with Android NDK <version>, but the following plugin(s) depend on a different Android NDK version.

The reason you met get this error is because it is a requirement of the `google_sign_in` library.

#### How to Fix:

1. Open the `android\app\build.gradle.kts` file.
2. Local the `ndkversion` property in the `android` block: android { ... ndkVersion = "<required-ndk-version>" }
3. Update the ndkVersion to match the version installed on your system, or install the required version (_yes_ this means hard-coding the NDK version number!)

#### To Check the Installed NDK Versions:

- Navigate to the NDK directory: C:\Users\<YourUsername>\AppData\Local\Android\Sdk\ndk
- The directory listing is the list of installed versions

#### To Install a Specific NDK Version:

- Easiest is to use Android Studio SDK Manager. The NDK versions are under the SDK Tools tab
- Otherwise, use the `sdkmanager` command: ```sdkmanager "ndk;<required-ndk-version>"```
  Replace `<required-ndk-version>` with the version specified in the error message.

