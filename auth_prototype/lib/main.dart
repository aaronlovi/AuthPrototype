import 'dart:developer';

import 'package:flutter/material.dart';
import 'package:google_sign_in/google_sign_in.dart';

void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Google Sign-In Demo',
      theme: ThemeData(primarySwatch: Colors.blue),
      home: GoogleSignInDemo(),
    );
  }
}

class GoogleSignInDemo extends StatefulWidget {
  const GoogleSignInDemo({super.key});

  @override
  GoogleSignInDemoState createState() => GoogleSignInDemoState();
}

class GoogleSignInDemoState extends State<GoogleSignInDemo> {
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

      setState(() {
        _user = account;
      });
    } catch (e) {
      log('Google sign-in error: $e');
    }
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
