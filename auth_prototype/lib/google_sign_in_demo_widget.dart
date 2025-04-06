import 'dart:convert';
import 'dart:developer';

import 'package:flutter/material.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;

class GoogleSignInDemoWidget extends StatefulWidget {
  const GoogleSignInDemoWidget({super.key});

  @override
  GoogleSignInDemoWidgetState createState() => GoogleSignInDemoWidgetState();
}

class GoogleSignInDemoWidgetState extends State<GoogleSignInDemoWidget> {
  final String _backendEndpoint = 'https://10.0.0.13:7137/api/auth/authenticate';
  final GoogleSignIn _googleSignIn = GoogleSignIn(scopes: ['email', 'profile']);

  GoogleSignInAccount? _user;

  Future<void> signInWithGoogle() async {
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

      http.Response response = await authenticateWithBackend(account);
      if (response.statusCode != 200) return;

      setState(() => _user = account);
    } catch (e) {
      log('Google sign-in error: $e');
      _showErrorDialog('Error', 'An error occurred during sign-in: $e');
    }
  }

  Future<http.Response> authenticateWithBackend(GoogleSignInAccount account) async {
    final GoogleSignInAuthentication auth = await account.authentication;

    final Map<String, String> requestBody = {
      'name': account.displayName ?? '',
      'email': account.email,
      'accessToken': auth.accessToken ?? '',
    };

    final response = await http.post(
      Uri.parse(_backendEndpoint),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(requestBody),
    );

    // Handle the response
    if (response.statusCode == 200) {
      final responseData = jsonDecode(response.body);
      log('Backend validated token successfully: $responseData');
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

  Future<void> signOut() async {
    await _googleSignIn.signOut();
    setState(() {
      _user = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Google Sign-In')),
      body: Center(
        child:
            _user == null
                ? ElevatedButton(
                  onPressed: signInWithGoogle,
                  child: Text('Sign in with Google'),
                )
                : Column(
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
                    ElevatedButton(onPressed: signOut, child: Text('Sign Out')),
                  ],
                ),
      ),
    );
  }
}
