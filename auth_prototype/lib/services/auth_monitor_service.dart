import 'dart:async';
import 'dart:developer';

import 'package:auth_prototype/services/auth_service.dart';
import 'package:get_it/get_it.dart';

class AuthMonitorService {
  final AuthService _authService = GetIt.I.get<AuthService>();
  Timer? _monitorTimer;

  void startMonitoring() {
    _monitorTimer?.cancel(); // Cancel any existing timer
    _monitorTimer = Timer.periodic(Duration(seconds: 15), (timer) {
      _checkForTokenChanges();
    });
  }

  void stopMonitoring() {
    _monitorTimer?.cancel(); // Stop the timer
    _monitorTimer = null;
  }

  Future<void> _checkForTokenChanges() async {
    final user = _authService.user;
    if (user == null) {
      log('No user is signed in. Stopping monitoring.');
      stopMonitoring();
      return;
    }

    try {
      final auth = await user.authentication;

      // Check if the access token has changed
      if (auth.accessToken != null && auth.accessToken != _authService.accessToken) {
        log('Access token has been updated automatically by Google Sign-In.');
        _authService.updateAccessToken(auth.accessToken!);
      } else {
        log('Access token has not changed.');
      }
    } catch (e) {
      log('Error checking for token updates: $e');
    }
  }
}
