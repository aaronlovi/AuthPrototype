import 'dart:convert';
import 'dart:developer';

import 'package:auth_prototype/utils/constants.dart';
import 'package:http/http.dart' as http;

class BackendAuthService {
  final String _backendEndpoint = Constants.backendEndpoint;

  Future<http.Response> authenticate({
    required String name,
    required String email,
    required String accessToken,
  }) async {
    final Map<String, String> requestBody = {
      'name': name,
      'email': email,
      'accessToken': accessToken,
    };

    final response = await http.post(
      Uri.parse('$_backendEndpoint/api/auth/authenticate'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(requestBody),
    );

    if (response.statusCode == 200) {
      log('Backend validated token successfully: ${response.body}');
    } else {
      log(
        'Backend authentication failed: ${response.statusCode} - ${response.body}',
      );
    }

    return response;
  }

  Future<http.Response> signOut(String email) async {
    final Map<String, String> requestBody = {'email': email};

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
    }

    return response;
  }
}
