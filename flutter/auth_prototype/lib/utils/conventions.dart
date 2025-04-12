import 'package:intl/intl.dart'; // Add this import

final DateFormat _formatter = DateFormat('yyyy-MM-dd h:mm:ss a');

class Conventions {
  static String formatDateTime(DateTime? dt) {
    if (dt == null) return 'N/A';
    return _formatter.format(dt.toLocal());
  }
}
