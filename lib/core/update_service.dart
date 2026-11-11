import 'dart:convert';

import 'package:http/http.dart' as http;

class UpdateResult {
  const UpdateResult({
    required this.success,
    required this.hasUpdate,
    required this.current,
    required this.latest,
    required this.releaseUrl,
    this.error,
  });

  final bool success;
  final bool hasUpdate;
  final String current;
  final String latest;
  final String releaseUrl;
  final String? error;
}

class UpdateService {
  static const currentVersion = '2.1.0';
  static const releasesUrl = 'https://github.com/CY68GI/PassiveWubi/releases';
  static const apiUrl =
      'https://api.github.com/repos/CY68GI/PassiveWubi/releases/latest';

  static Future<UpdateResult> check() async {
    try {
      final response = await http
          .get(
            Uri.parse(apiUrl),
            headers: {'User-Agent': 'PassiveWubi-Flutter/$currentVersion'},
          )
          .timeout(const Duration(seconds: 15));
      if (response.statusCode == 200) {
        final json = jsonDecode(response.body) as Map<String, dynamic>;
        return _resultFromTag(
          json['tag_name']?.toString(),
          json['html_url']?.toString(),
        );
      }
    } catch (_) {}
    try {
      final response = await http
          .get(Uri.parse('$releasesUrl/latest'))
          .timeout(const Duration(seconds: 15));
      return _resultFromTag(
        response.request?.url.pathSegments.last,
        response.request?.url.toString(),
      );
    } catch (error) {
      return UpdateResult(
        success: false,
        hasUpdate: false,
        current: currentVersion,
        latest: '',
        releaseUrl: releasesUrl,
        error: '网络连接失败，请稍后重试。',
      );
    }
  }

  static UpdateResult _resultFromTag(String? tag, String? url) {
    final normalized = (tag ?? '').replaceFirst(RegExp('^[vV]'), '');
    final current = VersionTuple.parse(currentVersion)!;
    final latest = VersionTuple.parse(normalized);
    return UpdateResult(
      success: latest != null,
      hasUpdate: latest != null && latest.isNewerThan(current),
      current: currentVersion,
      latest: latest?.toString() ?? '',
      releaseUrl: url ?? releasesUrl,
      error: latest == null ? '暂时无法获取最新版本。' : null,
    );
  }
}

class VersionTuple {
  const VersionTuple(this.major, this.minor, this.patch);
  final int major;
  final int minor;
  final int patch;

  static VersionTuple? parse(String input) {
    final match = RegExp(r'(\d+)\.(\d+)\.(\d+)').firstMatch(input);
    if (match == null) return null;
    return VersionTuple(
      int.parse(match.group(1)!),
      int.parse(match.group(2)!),
      int.parse(match.group(3)!),
    );
  }

  bool isNewerThan(VersionTuple other) =>
      major > other.major ||
      (major == other.major &&
          (minor > other.minor ||
              (minor == other.minor && patch > other.patch)));

  @override
  String toString() => '$major.$minor.$patch';
}
