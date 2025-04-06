import 'dart:developer';

import 'package:auth_prototype/services/auth_service.dart';
import 'package:auth_prototype/utils/conventions.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';

class GoogleSignInDemoWidget extends StatefulWidget {
  const GoogleSignInDemoWidget({super.key});

  @override
  GoogleSignInDemoWidgetState createState() => GoogleSignInDemoWidgetState();
}

class GoogleSignInDemoWidgetState extends State<GoogleSignInDemoWidget> {
  final AuthService _authService = GetIt.instance<AuthService>();

  String? get _userPhotoUrl => _authService.user?.photoUrl;
  String get _userDisplayName => _authService.user?.displayName ?? 'N/A';
  String get _userEmail => _authService.user?.email ?? 'N/A';
  String get _userAccessToken => _authService.accessToken;
  String get _userTokenExpiration =>
      Conventions.formatDateTime(_authService.tokenExpiration);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Google Sign-In')),
      body: Center(
        child:
            _authService.user == null
                ? _userNotSignedInBody()
                : _userSignedInBody(),
      ),
    );
  }

  Widget _userSignedInBody() => Column(
    mainAxisAlignment: MainAxisAlignment.center,
    children: [
      if (_userPhotoUrl != null)
        CircleAvatar(backgroundImage: NetworkImage(_userPhotoUrl!), radius: 40),
      SizedBox(height: 16),
      Text('Name: $_userDisplayName'),
      Text('Email: $_userEmail'),
      SizedBox(height: 16),
      Text('Access Token: $_userAccessToken'),
      SizedBox(height: 16),
      Text('Token Expiration: $_userTokenExpiration'),
      SizedBox(height: 16),
      ElevatedButton(onPressed: _signOut, child: Text('Sign Out')),
      ElevatedButton(
        onPressed: _forceTokenRefresh,
        child: Text('Force Token Refresh'),
      ),
    ],
  );

  Widget _userNotSignedInBody() =>
      ElevatedButton(onPressed: _signIn, child: Text('Sign in with Google'));

  void _showErrorDialog(String title, String message) {
    if (!mounted) return;

    showDialog(
      context: context,
      builder:
          (context) => AlertDialog(
            title: Text(title),
            content: Text(message),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: Text('OK'),
              ),
            ],
          ),
    );
  }

  void _signOut() async {
    try {
      await _authService.signOut();
      setState(() {});
    } catch (e) {
      log('Error signing out: $e');
      _showErrorDialog('Sign-Out Error', 'Failed to sign out: $e');
    }
  }

  void _forceTokenRefresh() async {
    try {
      await _authService.refreshGoogleToken();
      setState(() {});
    } catch (e) {
      log('Error refreshing token: $e');
      _showErrorDialog('Token Refresh Error', 'Failed to refresh token: $e');
    }
  }

  void _signIn() async {
    try {
      await _authService.signInWithGoogle();
      setState(() {});
    } catch (e) {
      log('Error signing in: $e');
      _showErrorDialog('Sign-In Error', 'Failed to sign in with Google: $e');
    }
  }
}
