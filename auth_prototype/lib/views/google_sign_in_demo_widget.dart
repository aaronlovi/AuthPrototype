import 'dart:convert';
import 'dart:developer';

import 'package:auth_prototype/utils/display_format.dart';
import 'package:flutter/material.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;

class GoogleSignInDemoWidget extends StatefulWidget {
  const GoogleSignInDemoWidget({super.key});

  @override
  GoogleSignInDemoWidgetState createState() => GoogleSignInDemoWidgetState();
}

class GoogleSignInDemoWidgetState extends State<GoogleSignInDemoWidget> {
  final String _backendEndpoint = 'https://10.0.0.13:7137';
  final GoogleSignIn _googleSignIn = GoogleSignIn(scopes: ['email', 'profile']);

  GoogleSignInAccount? _user;
  String _accessToken = '';
  DateTime? _tokenExpiration;
  String get tokenExpirationStr => Conventions.formatDateTime(_tokenExpiration);

  Future<void> _signInWithGoogle() async {
    try {
      final GoogleSignInAccount? account = await _googleSignIn.signIn();
      if (account == null) {
        log('User canceled sign-in');
        return; // user cancelled
      }

      log('User signed in:');
      log('Display Name: ${account.displayName}');
      log('Email: ${account.email}');
      log('Photo URL: ${account.photoUrl}');
      log('ID: ${account.id}');

      final GoogleSignInAuthentication auth = await account.authentication;
      http.Response response = await _authenticateWithBackend(account, auth);
      if (response.statusCode != 200) return;

      setState(() {
        _user = account;
        _accessToken = auth.accessToken ?? '';
      });
    } catch (e) {
      log('Google sign-in error: $e');
      _showErrorDialog('Error', 'An error occurred during sign-in: $e');
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

    // Handle the response
    if (response.statusCode == 200) {
      final responseData = jsonDecode(response.body);
      log('Backend validated token successfully: $responseData');
      setState(() {
        _tokenExpiration = DateTime.parse(responseData['expirationDateTime']);
      });
    } else if (response.statusCode == 400) {
      log('Bad request: ${response.body}');
      _showErrorDialog('Error', 'Bad request: ${response.body}');
    } else if (response.statusCode == 401) {
      log('Unauthorized: ${response.body}');
      _showErrorDialog('Error', 'Unauthorized: ${response.body}');
    } else {
      log('Unexpected error: ${response.statusCode} - ${response.body}');
      _showErrorDialog('Error', 'Unexpected error: ${response.body}');
    }

    return response;
  }

  Future<void> _signOut() async {
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
        log(
          'Failed to sign out on the backend: ${response.statusCode} - ${response.body}',
        );
        _showErrorDialog(
          'Error',
          'Failed to sign out on the backend: ${response.body}',
        );
      }

      setState(() {
        _user = null;
        _accessToken = '';
        _tokenExpiration = null;
      });
    } catch (e) {
      log('Sign-out error: $e');
      _showErrorDialog('Error', 'An error occurred during sign-out: $e');
    }
  }

  Future<void> _refreshGoogleToken() async {
    try {
      // Sign out the user
      await _googleSignIn.disconnect();

      // Sign back in to get a new token
      final GoogleSignInAccount? account = await _googleSignIn.signIn();
      if (account == null) {
        log('User canceled re-sign-in');
        return; // User canceled
      }

      final GoogleSignInAuthentication auth = await account.authentication;
      // Re-authenticate with the backend
      final http.Response response = await _authenticateWithBackend(
        account,
        auth,
      );

      if (response.statusCode != 200) {
        log('Failed to re-authenticate with the backend');
        return;
      }

      setState(() {
        _user = account;
        _accessToken = auth.accessToken ?? '';
      });

      log('Token refreshed successfully');
      log('New Access Token: $_accessToken');
    } catch (e) {
      log('Error refreshing token: $e');
      _showErrorDialog(
        'Error',
        'An error occurred while refreshing the token: $e',
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Google Sign-In')),
      body: Center(
        child: _user == null ? _userNotSignedInBody() : _userSignedInBody(),
      ),
    );
  }

  Widget _userSignedInBody() => Column(
    mainAxisAlignment: MainAxisAlignment.center,
    children: [
      if (_user!.photoUrl != null)
        CircleAvatar(
          backgroundImage: NetworkImage(_user!.photoUrl!),
          radius: 40,
        ),
      SizedBox(height: 16),
      Text('Name: ${_user!.displayName ?? 'N/A'}'),
      Text('Email: ${_user!.email}'),
      SizedBox(height: 16),
      Text('Access Token: $_accessToken'),
      SizedBox(height: 16),
      Text('Token Expiration: $tokenExpirationStr'),
      SizedBox(height: 16),
      ElevatedButton(onPressed: _signOut, child: Text('Sign Out')),
      ElevatedButton(
        onPressed: _refreshGoogleToken,
        child: Text('Refresh Token'),
      ),
    ],
  );

  Widget _userNotSignedInBody() => ElevatedButton(
    onPressed: _signInWithGoogle,
    child: Text('Sign in with Google'),
  );

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
}
