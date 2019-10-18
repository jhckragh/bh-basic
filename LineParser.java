import java.util.*;

public class LineParser {
    private final List<Token> line;
    private final Token eol;
    private int cursor;
    private int writtenLineNumber;

    public LineParser(List<Token> line) {
        this.line = appendEol(line);
        this.eol = this.line.get(this.line.size() - 1);
        this.cursor = 0;
    }

    public Command parse() {
        Token first = getCurrentToken();
        if (first.getKind() != Token.Kind.INTEGER_LITERAL) {
            syntaxError("line must start with a line number");
        }
        writtenLineNumber = Integer.parseInt(first.getText());
        acceptIt();
        return parseCommand();
    }

    private Command parseCommand() {
        if (getCurrentToken().getKind() == Token.Kind.EOL) {
            return new Command.Empty(writtenLineNumber);
        }
        switch (getCurrentToken().getText()) {
            case "let": return parseLet();
            case "print": return parsePrint();
            case "input": return parseInput();
            case "for": return parseFor();
            case "next": return parseNext();
            case "if": return parseIf();
            case "goto": return parseGoto();
            case "gosub": return parseGoSub();
            case "return": return parseReturn();
            case "end": return parseEnd();
            default: syntaxError(
                "expected a keyword (let, print, ...) but saw " +
                    getCurrentToken().getText()
            );
        }
        return null; // shouldn't be reachable, but javac insists
    }

    private Command.Let parseLet() {
        acceptIt();
        Token identifier = accept(Token.Kind.IDENTIFIER);
        accept(Token.Kind.EQ);
        Expression exp = parseExpression();
        accept(Token.Kind.EOL);
        return new Command.Let(writtenLineNumber, identifier.getText(), exp);
    }

    private Command.Print parsePrint() {
        acceptIt();
        List<Object> values = new ArrayList<>();
        while (getCurrentToken().getKind() != Token.Kind.EOL) {
            switch (getCurrentToken().getKind()) {
                case STRING_LITERAL:
                case COMMA:
                case SEMICOLON:
                    values.add(acceptIt());
                    break;
                default:
                    values.add(parseExpression());
                    break;
            }
        }
        accept(Token.Kind.EOL);
        return new Command.Print(writtenLineNumber, values);
    }

    private Command.Input parseInput() {
        acceptIt();
        String prompt = null;
        if (getCurrentToken().getKind() == Token.Kind.STRING_LITERAL) {
            prompt = acceptIt().getText();
        }
        List<String> variableNames = new ArrayList<>();
        variableNames.add(accept(Token.Kind.IDENTIFIER).getText());
        while (getCurrentToken().getKind() == Token.Kind.COMMA) {
            acceptIt();
            variableNames.add(accept(Token.Kind.IDENTIFIER).getText());
        }
        accept(Token.Kind.EOL);
        return new Command.Input(writtenLineNumber, prompt, variableNames);
    }

    private Command.For parseFor() {
        acceptIt();
        Token identifier = accept(Token.Kind.IDENTIFIER);
        accept(Token.Kind.EQ);
        Expression from = parseExpression();
        accept(Token.Kind.TO);
        Expression to = parseExpression();
        accept(Token.Kind.EOL);
        return new Command.For(writtenLineNumber, identifier.getText(), from, to);
    }

    private Command.Next parseNext() {
        acceptIt();
        Token identifier = accept(Token.Kind.IDENTIFIER);
        accept(Token.Kind.EOL);
        return new Command.Next(writtenLineNumber, identifier.getText());
    }

    private Command.If parseIf() {
        acceptIt();
        BooleanExpression exp = parseBooleanExpression();
        accept(Token.Kind.THEN);
        Command cmd = parseCommand();
        return new Command.If(writtenLineNumber, exp, cmd);
    }

    private Command.Goto parseGoto() {
        acceptIt();
        Token target = accept(Token.Kind.INTEGER_LITERAL);
        accept(Token.Kind.EOL);
        return new Command.Goto(writtenLineNumber,
            Integer.parseInt(target.getText()));
    }

    private Command.GoSub parseGoSub() {
        acceptIt();
        Token target = accept(Token.Kind.INTEGER_LITERAL);
        accept(Token.Kind.EOL);
        return new Command.GoSub(writtenLineNumber,
            Integer.parseInt(target.getText()));
    }

    private Command.Return parseReturn() {
        acceptIt();
        accept(Token.Kind.EOL);
        return new Command.Return(writtenLineNumber);
    }

    private Command.End parseEnd() {
        acceptIt();
        accept(Token.Kind.EOL);
        return new Command.End(writtenLineNumber);
    }

    private BooleanExpression parseBooleanExpression() {
        Expression left = parseExpression();
        Token t = getCurrentToken();
        Operator op = null;
        if (t.getKind() == Token.Kind.LT) {
            op = Operator.LT;
        } else if (t.getKind() == Token.Kind.EQ) {
            op = Operator.EQ;
        } else if (t.getKind() == Token.Kind.GT) {
            op = Operator.GT;
        } else {
            syntaxError("expected '<', '=' or '>', but saw " + t.getText());
        }
        acceptIt();
        Expression right = parseExpression();
        return new BooleanExpression(left, op, right);
    }

    private Expression parseExpression() {
        Expression term1 = parseTerm();
        while (getCurrentToken().getKind() == Token.Kind.PLUS ||
            getCurrentToken().getKind() == Token.Kind.MINUS) {
            Operator op = null;
            if (getCurrentToken().getKind() == Token.Kind.PLUS) {
                op = Operator.PLUS;
            } else {
                op = Operator.MINUS;
            }
            acceptIt();
            Expression term2 = parseTerm();
            term1 = new Expression.Binary(term1, op, term2);
        }
        return term1;
    }

    private Expression parseTerm() {
        Expression factor1 = parseFactor();
        while (getCurrentToken().getKind() == Token.Kind.TIMES ||
            getCurrentToken().getKind() == Token.Kind.DIV) {
            Operator op = null;
            if (getCurrentToken().getKind() == Token.Kind.TIMES) {
                op = Operator.TIMES;
            } else {
                op = Operator.DIV;
            }
            acceptIt();
            Expression factor2 = parseFactor();
            factor1 = new Expression.Binary(factor1, op, factor2);
        }
        return factor1;
    }

    private Expression parseFactor() {
        if (getCurrentToken().getKind() == Token.Kind.IDENTIFIER) {
            String variableName = acceptIt().getText();
            return new Expression.Identifier(variableName);
        } else if (getCurrentToken().getKind() == Token.Kind.INTEGER_LITERAL) {
            int value = Integer.parseInt(acceptIt().getText());
            return new Expression.Literal(value);
        } else if (getCurrentToken().getKind() == Token.Kind.L_PAREN) {
            acceptIt();
            Expression exp = parseExpression();
            accept(Token.Kind.R_PAREN);
            return exp;
        } else if (getCurrentToken().getKind() == Token.Kind.MINUS) {
            acceptIt();
            Expression exp = parseFactor();
            return new Expression.Binary(new Expression.Literal(0), Operator.MINUS, exp);
        } else {
            syntaxError("expected variable, integer, '(' or '-', but saw " +
                getCurrentToken().getText());
        }
        return null; // shouldn't be reachable, but javac insists
    }

    private Token getCurrentToken() {
        if (cursor > line.size()) {
            return eol;
        }
        return line.get(cursor);
    }

    private Token acceptIt() {
        Token t = getCurrentToken();
        cursor++;
        return t;
    }

    private Token accept(Token.Kind kind) {
        Token t = getCurrentToken();
        if (t.getKind() != kind) {
            syntaxError(String.format("expected %s but saw %s",
                kind, t.getKind()));
        }
        cursor++;
        return t;
    }

    private List<Token> appendEol(List<Token> line) {
        List<Token> prepped = new ArrayList<>(line);
        if (prepped.isEmpty()) {
            prepped.add(new Token(Token.Kind.EOL, "<eol>", 0, 0));
        } else {
            Token last = prepped.get(prepped.size() - 1);
            if (last.getKind() != Token.Kind.EOL) {
                int col = last.getColumnNumber() + last.getText().length();
                prepped.add(new Token(Token.Kind.EOL, "<eol>", last.getLineNumber(), col));
            }
        }
        return prepped;
    }

    private void syntaxError(String msg) {
        Token t = getCurrentToken();
        String fullMsg = String.format("[%d:%d] syntax error: %s",
            t.getLineNumber(), t.getColumnNumber(), msg);
        throw new RuntimeException(fullMsg);
    }
}