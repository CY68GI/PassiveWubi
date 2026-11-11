import 'dart:async';
import 'dart:io';

class SingleInstance {
  static const _port = 47832;
  static ServerSocket? _server;
  static final StreamController<void> _activationController =
      StreamController<void>.broadcast();

  static Stream<void> get activations => _activationController.stream;

  static Future<bool> acquire() async {
    try {
      _server = await ServerSocket.bind(InternetAddress.loopbackIPv4, _port);
      _server!.listen((socket) {
        socket.write('show');
        socket.close();
        _activationController.add(null);
      });
      return true;
    } on SocketException {
      try {
        final socket = await Socket.connect(
          InternetAddress.loopbackIPv4,
          _port,
          timeout: const Duration(milliseconds: 700),
        );
        socket.write('show');
        await socket.flush();
        await socket.close();
      } catch (_) {}
      return false;
    }
  }
}
