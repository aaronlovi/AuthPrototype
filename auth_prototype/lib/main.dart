import 'dart:io';

import 'package:auth_prototype/services/auth_service.dart';
import 'package:auth_prototype/utils/development_http_overrides.dart';
import 'package:auth_prototype/views/google_sign_in_demo_widget.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';

void main() {
  if (!kReleaseMode) {
    HttpOverrides.global =
        DevelopmentHttpOverrides(); // Override SSL verification
  }

  GetIt.I.registerSingleton<AuthService>(AuthService());

  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Google Sign-In Demo',
      theme: ThemeData(primarySwatch: Colors.blue),
      home: GoogleSignInDemoWidget(),
    );
  }
}
