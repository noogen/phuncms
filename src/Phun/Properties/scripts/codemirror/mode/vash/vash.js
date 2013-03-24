CodeMirror.defineMode("vash", function (config, parserConfig) {

    var regexVashKeywords = /^[html\.]*(?:abstract|append|as|base|bool|block|break|byte|case|catch|char|checked|class|const|continue|DateTime|decimal|default|delegate|do|double|dynamic|else|enum|escape|event|explicit|extend|extern|false|finally|fixed|float|for|foreach|goto|helper|if|implicit|in|include|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|prepend|private|protected|public|raw|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|var|virtual|void|volatile|while)$/im;

    var vashOverlay = {

        startState: function () {
            return { inVashBlock: false };
        },

        token: function (stream, state) {
			var ch = stream.next();
			
			// text mode
			if (ch == "@") {
				if (stream.peek() != "@") { // handle @@ escaping
					state.inVashBlock = true;
					return "vash-tag";
				}
			} else if (state.inVashBlock) {
				while (!stream.eol()) {
					ch = stream.peek();
					if (/[\.a-zA-Z]/.test(ch)) {
						if (/[\.a-zA-Z]/.test(stream.current())) {
							stream.eatWhile(/[\.a-zA-Z]/);
							if (regexVashKeywords.test(stream.current())) {
								return "vash-keyword";
							}
						}
						return "vash";
					}
					else if (/\s/.test(ch))
					{
						return "vash";
					}
					ch = stream.next();
				}
				
				// for now online support keyword in the same line
				state.inVashBlock = false;
				return "vash"
			}
			
			while (stream.next() != null && !stream.match("@", false)) {}
			return;
        },

        indent: function (state, textAfter) {
            var context = state.context;
            if (context && context.noIndent) return 0;
            if (alignCDATA && /<!\[CDATA\[/.test(textAfter)) return 0;
            if (context && /^<\//.test(textAfter))
                context = context.prev;
            while (context && !context.startOfLine)
                context = context.prev;
            if (context) return context.indent + indentUnit;
            else return 0;
        },

        electricChars: "/"
    };
	return CodeMirror.overlayMode(CodeMirror.getMode(config, parserConfig.backdrop || "text/html"), vashOverlay);
});