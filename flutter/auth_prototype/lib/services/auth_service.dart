import 'dart:async';
import 'dart:convert';
import 'dart:developer';

import 'package:auth_prototype/services/auth_monitor_service.dart';
import 'package:auth_prototype/services/backend_auth_service.dart';
import 'package:flutter/foundation.dart';
import 'package:get_it/get_it.dart';
import 'package:google_sign_in/google_sign_in.dart';

class AuthService extends ChangeNotifier {
  final BackendAuthService _backendAuthService = BackendAuthService();
  final GoogleSignIn _googleSignIn = GoogleSignIn(scopes: ['email', 'profile']);

  GoogleSignInAccount? _user;
  String _accessToken = '';
  DateTime? _tokenExpiration;

  GoogleSignInAccount? get user => _user;
  String get accessToken => _accessToken;
  DateTime? get tokenExpiration => _tokenExpiration;

  @override
  void dispose() {
    super.dispose();
    final AuthMonitorService monitorService = GetIt.I.get<AuthMonitorService>();
    monitorService.dispose();
    log('AuthService disposed.');
  }

  Future<void> updateAccessToken(String newAccessToken, {required bool forceReauth}) async {
    if (accessToken == newAccessToken && !forceReauth) {
      log('Access token is the same, no update needed.');
      return Future.value();
    }

    _accessToken = newAccessToken;
    log('Access token updated: $_accessToken');

    final response = await _backendAuthService.authenticate(
      name: _user?.displayName ?? '',
      email: _user?.email ?? '',
      accessToken: _accessToken,
    );

    if (response.statusCode == 200) {
      final responseData = jsonDecode(response.body);
      _tokenExpiration = DateTime.parse(responseData['expirationDateTime']);
      log('Token expiration updated: $_tokenExpiration');
    }

    notifyListeners();
  }

  Future<void> signInWithGoogle() async {
    try {
      final GoogleSignInAccount? account = await _googleSignIn.signIn();
      if (account == null) {
        log('User canceled sign-in');
        return; // User canceled
      }

      final GoogleSignInAuthentication auth = await account.authentication;
      final response = await _backendAuthService.authenticate(
        name: account.displayName ?? '',
        email: account.email,
        accessToken: auth.accessToken ?? '',
      );

      if (response.statusCode == 200) {
        final responseData = jsonDecode(response.body);
        _user = account;
        _accessToken = auth.accessToken ?? '';
        _tokenExpiration = DateTime.parse(responseData['expirationDateTime']);

        final AuthMonitorService monitorService =
            GetIt.I.get<AuthMonitorService>();
        monitorService.startMonitoring();

        notifyListeners();
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

      final response = await _backendAuthService.signOut(_user!.email);

      if (response.statusCode == 200) {
        _user = null;
        _accessToken = '';
        _tokenExpiration = null;

        final AuthMonitorService monitorService =
            GetIt.I.get<AuthMonitorService>();
        monitorService.stopMonitoring();

        notifyListeners();
      }
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

      final response = await _backendAuthService.authenticate(
        name: account.displayName ?? '',
        email: account.email,
        accessToken: auth.accessToken ?? '',
      );

      if (response.statusCode == 200) {
        final responseData = jsonDecode(response.body);
        _user = account;
        _accessToken = auth.accessToken ?? '';
        _tokenExpiration = DateTime.parse(responseData['expirationDateTime']);

        log('Token refreshed successfully. New access token: $_accessToken');

        final AuthMonitorService monitorService =
            GetIt.I.get<AuthMonitorService>();
        monitorService.startMonitoring();

        notifyListeners();
      }
    } catch (e) {
      log('Error refreshing token: $e');
      rethrow;
    }
  }
}
