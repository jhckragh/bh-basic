public class Token {
    public enum Kind {
        IDENTIFIER, EQ, LET, PRINT, COMMA, SEMICOLON, INPUT, FOR,
        TO, NEXT, IF, THEN, GOTO, GOSUB, RETURN, END, STRING_LITERAL,
        INTEGER_LITERAL, PLUS, MINUS, TIMES, DIV, LT, GT, L_PAREN,
        R_PAREN, COLON, EOL, ERROR
    }

    private final int columnNumber;
    private final int lineNumber;
    private final Kind kind;
    private final String text;

    public Token(Kind kind, String text, int lineNumber, int columnNumber) {
        this.kind = kind;
        this.text = text;
        this.lineNumber = lineNumber;
        this.columnNumber = columnNumber;
    }

    public int getColumnNumber() {
        return columnNumber;
    }

    public int getLineNumber() {
        return lineNumber;
    }

    public Kind getKind() {
        return kind;
    }

    public String getText() {
        return text;
    }

    public String toString() {
        return String.format("Token[%s, '%s', %d, %d]",
            kind, text, lineNumber, columnNumber);
    }

    public static Kind lookupKeyword(String spelling) {
        switch (spelling) {
            case "let": return Kind.LET;
            case "print": return Kind.PRINT;
            case "input": return Kind.INPUT;
            case "for": return Kind.FOR;
            case "to": return Kind.TO;
            case "next": return Kind.NEXT;
            case "if": return Kind.IF;
            case "then": return Kind.THEN;
            case "goto": return Kind.GOTO;
            case "gosub": return Kind.GOSUB;
            case "return": return Kind.RETURN;
            case "end": return Kind.END;
            default: return null;
        }
    }

    public static Kind lookupSymbol(char c) {
        switch (c) {
            case '=': return Kind.EQ;
            case ',': return Kind.COMMA;
            case ';': return Kind.SEMICOLON;
            case ':': return Kind.COLON;
            case '+': return Kind.PLUS;
            case '-': return Kind.MINUS;
            case '*': return Kind.TIMES;
            case '/': return Kind.DIV;
            case '<': return Kind.LT;
            case '>': return Kind.GT;
            case '(': return Kind.L_PAREN;
            case ')': return Kind.R_PAREN;
            default: return null;
        }
    }
}