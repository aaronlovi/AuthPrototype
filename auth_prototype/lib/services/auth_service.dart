import 'dart:convert';
import 'dart:developer';

import 'package:auth_prototype/utils/constants.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;

class AuthService {
  final String _backendEndpoint = Constants.backendEndpoint;
  final GoogleSignIn _googleSignIn = GoogleSignIn(scopes: ['email', 'profile']);

  GoogleSignInAccount? _user;
  String _accessToken = '';
  DateTime? _tokenExpiration;

  GoogleSignInAccount? get user => _user;
  String get accessToken => _accessToken;
  DateTime? get tokenExpiration => _tokenExpiration;

  Future<void> signInWithGoogle() async {
    try {
      final GoogleSignInAccount? account = await _googleSignIn.signIn();
      if (account == null) {
        log('User canceled sign-in');
        return; // User canceled
      }

      final GoogleSignInAuthentication auth = await account.authentication;
      final http.Response response = await _authenticateWithBackend(account, auth);

      if (response.statusCode == 200) {
        _user = account;
        _accessToken = auth.accessToken ?? '';
      }
    } catch (e) {
      log('Google sign-in error: $e');
      rethrow;
    }
  }

  Future<void> signOut() async {
    try {
      if (_user == null) return;

      await _googleSignIn.signOut();

      final Map<String, String> requestBody = {'email': _user!.email};
      final response = await http.post(
        Uri.parse('$_backendEndpoint/api/auth/signout'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode(requestBody),
      );

      if (response.statusCode == 200) {
        log('Successfully signed out on the backend.');
      } else {
        log('Failed to sign out on the backend: ${response.statusCode} - ${response.body}');
      }

      _user = null;
      _accessToken = '';
      _tokenExpiration = null;
    } catch (e) {
      log('Sign-out error: $e');
      rethrow;
    }
  }

  Future<void> refreshGoogleToken() async {
    try {
      await _googleSignIn.disconnect();
      final GoogleSignInAccount? account = await _googleSignIn.signIn();
      if (account == null) {
        log('User canceled re-sign-in');
        return; // User canceled
      }

      final GoogleSignInAuthentication auth = await account.authentication;
      final http.Response response = await _authenticateWithBackend(account, auth);

      if (response.statusCode == 200) {
        _user = account;
        _accessToken = auth.accessToken ?? '';
      }
    } catch (e) {
      log('Error refreshing token: $e');
      rethrow;
    }
  }

  Future<http.Response> _authenticateWithBackend(
    GoogleSignInAccount account,
    GoogleSignInAuthentication auth,
  ) async {
    final Map<String, String> requestBody = {
      'name': account.displayName ?? '',
      'email': account.email,
      'accessToken': auth.accessToken ?? '',
    };

    final response = await http.post(
      Uri.parse('$_backendEndpoint/api/auth/authenticate'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(requestBody),
    );

    if (response.statusCode == 200) {
      final responseData = jsonDecode(response.body);
      log('Backend validated token successfully: $responseData');
      _tokenExpiration = DateTime.parse(responseData['expirationDateTime']);
    } else {
      log('Backend authentication failed: ${response.statusCode} - ${response.body}');
    }

    return response;
  }
}