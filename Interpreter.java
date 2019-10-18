import java.io.IOException;
import java.nio.charset.*;
import java.nio.file.Path;
import java.util.*;

public class Interpreter {
    private int currentLine;
    private final Deque<Command.RealizedFor> forCmds;
    private final Deque<Command.GoSub> goSubCmds;
    private final SortedMap<Integer, Command> program;
    private final Scanner scanner;
    private final Map<String, Integer> variables;

    public Interpreter() {
        forCmds = new ArrayDeque<>();
        goSubCmds = new ArrayDeque<>();
        program = new TreeMap<>();
        scanner = new Scanner(System.in);
        variables = new HashMap<>();
    }

    public void run(String source) {
        lexAndParse(source);
        currentLine = program.firstKey();
        while (!(getCurrentCommand() instanceof Command.End)) {
            run(getCurrentCommand());
        }
    }

    private void run(Command command) {
        if (command instanceof Command.Let) {
            runLet((Command.Let) command);
        } else if (command instanceof Command.Print) {
            runPrint((Command.Print) command);
        } else if (command instanceof Command.Input) {
            runInput((Command.Input) command);
        } else if (command instanceof Command.For) {
            runFor((Command.For) command);
        } else if (command instanceof Command.Next) {
            runNext((Command.Next) command);
        } else if (command instanceof Command.If) {
            runIf((Command.If) command);
        } else if (command instanceof Command.Goto) {
            runGoto((Command.Goto) command);
        } else if (command instanceof Command.GoSub) {
            runGoSub((Command.GoSub) command);
        } else if (command instanceof Command.Return) {
            runReturn((Command.Return) command);
        } else if (command instanceof Command.Empty) {
            runEmpty((Command.Empty) command);
        } else if (command instanceof Command.End) {
            // do nothing
        } else {
            throw new RuntimeException("unsupported command: " + command);
        }
    }

    private void runLet(Command.Let cmd) {
        int val = eval(cmd.getRhs());
        variables.put(cmd.getLhs(), val);
        step();
    }

    private void runPrint(Command.Print cmd) {
        for (Object v : cmd.getValues()) {
            if (v instanceof Token) {
                Token t = (Token) v;
                if (t.getKind() == Token.Kind.STRING_LITERAL) {
                    System.out.print(t.getText());
                } else if (t.getKind() == Token.Kind.SEMICOLON) {
                    // do nothing
                } else if (t.getKind() == Token.Kind.COMMA) {
                    System.out.print("\t");
                } else {
                    throw new RuntimeException("internal error");
                }
            } else if (v instanceof Expression) {
                System.out.print(eval((Expression) v));
            } else {
                throw new RuntimeException("internal error");
            }
        }
        System.out.println();
        step();
    }

    private void runInput(Command.Input cmd) {
        if (cmd.getPrompt() != null) {
            System.out.print(cmd.getPrompt());
        }
        for (String variableName : cmd.getVariableNames()) {
            int val = scanner.nextInt();
            variables.put(variableName, val);
        }
        step();
    }

    private void runFor(Command.For cmd) {
        String variableName = cmd.getVariableName();
        int from = eval(cmd.getFrom());
        int to = eval(cmd.getTo());
        variables.put(variableName, from);
        forCmds.addFirst(new Command.RealizedFor(cmd.getLineNumber(), variableName, from, to));
        step();
    }

    private void runNext(Command.Next cmd) {
        String variableName = cmd.getVariableName();
        Command.RealizedFor parentFor = forCmds.getFirst();
        if (!parentFor.getVariableName().equals(variableName)) {
            throw new RuntimeException("incorrect nesting of FORs");
        }
        int counterValue = variables.get(variableName);
        if (counterValue >= parentFor.getTo()) {
            forCmds.removeFirst();
            step();
        } else {
            variables.put(variableName, counterValue + 1);
            currentLine = program.tailMap(parentFor.getLineNumber() + 1).firstKey();
        }        
    }

    private void runIf(Command.If cmd) {
        boolean runThen = eval(cmd.getExp());
        if (runThen) {
            run(cmd.getCmd());
        } else {
            step();
        }
    }

    private void runGoto(Command.Goto cmd) {
        currentLine = cmd.getTargetLine();
    }

    private void runGoSub(Command.GoSub cmd) {
        goSubCmds.push(cmd);
        currentLine = cmd.getTargetLine();
    }

    private void runReturn(Command.Return cmd) {
        Command.GoSub parent = goSubCmds.removeFirst();
        currentLine = program.tailMap(parent.getLineNumber() + 1).firstKey();
    }

    private void runEmpty(Command.Empty cmd) {
        step();
    }

    private void step() {
        currentLine = program.tailMap(currentLine + 1).firstKey();
    }

    private int eval(Expression exp) {
        if (exp instanceof Expression.Identifier) {
            Expression.Identifier ident = (Expression.Identifier) exp;
            return variables.get(ident.getVariableName());
        } else if (exp instanceof Expression.Literal) {
            Expression.Literal lit = (Expression.Literal) exp;
            return lit.getValue();
        } else if (exp instanceof Expression.Binary) {
            Expression.Binary bin = (Expression.Binary) exp;
            int left = eval(bin.getLeft());
            int right = eval(bin.getRight());
            switch (bin.getOperator()) {
                case PLUS: return left + right;
                case MINUS: return left - right;
                case TIMES: return left * right;
                case DIV: return left / right;
                default: throw new RuntimeException("unsupported arithmetic operator: " + bin.getOperator());
            }
        }
        throw new RuntimeException("internal error");
    }

    private boolean eval(BooleanExpression exp) {
        int left = eval(exp.getLeft());
        int right = eval(exp.getRight());
        switch (exp.getOperator()) {
            case LT: return left < right;
            case EQ: return left == right;
            case GT: return left > right;
            default: throw new RuntimeException("unsupported relational operator: " + exp.getOperator());
        }
    }

    private Command getCurrentCommand() {
        return program.get(currentLine);
    }

    private void lexAndParse(String source) {
        int lineNumber = 1;
        for (String line : source.split("\n")) {
            LineLexer lexer = new LineLexer(line, lineNumber);
            List<Token> tokens = new ArrayList<>();
            while (true) {
                Token t = lexer.nextToken();
                tokens.add(t);
                if (t.getKind() == Token.Kind.EOL) {
                    break;
                }
            }

            LineParser parser = new LineParser(tokens);
            Command cmd = parser.parse();
            program.put(cmd.getLineNumber(), cmd);
        }
    }

    public static void main(String[] args) throws IOException {
        if (args.length != 1) {
            System.out.println("Usage: java Interpreter SOURCEFILE");
            System.exit(1);
        }
        
        Charset charset = StandardCharsets.UTF_8;
        Path path = Path.of(args[0]);
        Scanner in = new Scanner(path, charset);
        List<String> lines = new ArrayList<>();
        while (in.hasNextLine()) {
            lines.add(in.nextLine());
        }
        in.close();

        Interpreter interpreter = new Interpreter();
        interpreter.run(String.join("\n", lines));
    }
}
