public class LineLexer {
    private int columnNumber;
    private final String line;
    private final int lineNumber;

    public LineLexer(String line, int lineNumber) {
        this.line = line;
        this.lineNumber = lineNumber;
        this.columnNumber = 0;
    }

    public Token nextToken() {
        while (Character.isWhitespace(getCurrentChar())) {
            columnNumber++;
        }
        if (getCurrentChar() == 0) {
            return new Token(Token.Kind.EOL, "<eol>", lineNumber, line.length() - 1);
        }

        int column = columnNumber;

        // Identifiers and keywords
        if (isIdentifierStart(getCurrentChar())) {
            StringBuilder sb = new StringBuilder();
            do {
                sb.append(getCurrentChar());
                columnNumber++;
            } while (isIdentifierPart(getCurrentChar()));
            Token.Kind kind = Token.lookupKeyword(sb.toString());
            if (kind == null) {
                kind = Token.Kind.IDENTIFIER;
            }
            return new Token(kind, sb.toString(), lineNumber, column);
        }

        // Integer literals
        if (getCurrentChar() == '0') {
            columnNumber++;
            return new Token(Token.Kind.INTEGER_LITERAL, "0", lineNumber, column);
        }
        if (Character.isDigit(getCurrentChar())) {
            StringBuilder sb = new StringBuilder();
            do {
                sb.append(getCurrentChar());
                columnNumber++;
            } while (Character.isDigit(getCurrentChar()));
            return new Token(Token.Kind.INTEGER_LITERAL, sb.toString(), lineNumber, column);
        }

        // String literals
        if (getCurrentChar() == '"') {
            columnNumber++; // Skip opening quote
            StringBuilder sb = new StringBuilder();
            while (getCurrentChar() != '"' && getCurrentChar() != 0) {
                if (getCurrentChar() == '\n') {
                    return new Token(Token.Kind.ERROR,
                        "line end in string literal", lineNumber, columnNumber);
                }
                sb.append(getCurrentChar());
                columnNumber++;
            }
            if (getCurrentChar() != '"') {
                return new Token(Token.Kind.ERROR,
                    "unclosed string literal", lineNumber, columnNumber);
            }
            columnNumber++; // Skip closing quote
            return new Token(Token.Kind.STRING_LITERAL,
                sb.toString(), lineNumber, columnNumber);
        }

        String text = Character.toString(getCurrentChar());
        Token.Kind kind = Token.lookupSymbol(getCurrentChar());
        columnNumber++;
        if (kind == null) {
            return new Token(Token.Kind.ERROR,
                "invalid character: '" + text + "'", lineNumber, column);
        }
        return new Token(kind, text, lineNumber, column);
    }

    private char getCurrentChar() {
        return (columnNumber < line.length()) ? line.charAt(columnNumber) : 0;
    }

    private boolean isIdentifierStart(char c) {
        return Character.isLetter(c) || c == '_' || c == '$';
    }

    private boolean isIdentifierPart(char c) {
        return isIdentifierStart(c) || Character.isDigit(c);
    }
}