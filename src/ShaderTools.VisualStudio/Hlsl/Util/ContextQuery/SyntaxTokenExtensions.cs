using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.Util.ContextQuery
{
    internal static class SyntaxTokenExtensions
    {
        // Copied from
        // https://github.com/dotnet/roslyn/blob/fb5d0b2a0daa2cbc66c88c1ff483b52d0ed4b9d5/src/Workspaces/CSharp/Portable/Extensions/ContextQuery/SyntaxTokenExtensions.cs#L51
        public static bool IsBeginningOfStatementContext(this SyntaxToken token)
        {
            // cases:
            //    {
            //      |

            // }
            // |

            // Note, the following is *not* a legal statement context: 
            //    do { } |

            // ...;
            // |

            // case 0:
            //   |

            // default:
            //   |

            // if (foo)
            //   |

            // while (true)
            //   |

            // do
            //   |

            // for (;;)
            //   |

            // else
            //   |

            // for ( ; ; Foo(), |

            if (token.Kind == SyntaxKind.OpenBraceToken &&
                token.Parent.IsKind(SyntaxKind.Block))
            {
                return true;
            }

            if (token.Kind == SyntaxKind.SemiToken)
            {
                var statement = token.GetAncestor<StatementSyntax>();
                if (statement != null && !statement.IsParentKind(SyntaxKind.GlobalStatement) &&
                    statement.GetLastToken(includeSkippedTokens: true) == token)
                {
                    return true;
                }
            }

            if (token.Kind == SyntaxKind.CloseBraceToken &&
                token.Parent.IsKind(SyntaxKind.Block))
            {
                if (token.Parent.Parent is StatementSyntax)
                {
                    // Most blocks that are the child of statement are places
                    // that we can follow with another statement.  i.e.:
                    // if { }
                    // while () { }
                    // There is one exception.
                    // do {}
                    if (!token.Parent.IsParentKind(SyntaxKind.DoStatement))
                    {
                        return true;
                    }
                }
                else if (
                    token.Parent.IsParentKind(SyntaxKind.ElseClause) ||
                    token.Parent.IsParentKind(SyntaxKind.SwitchSection))
                {
                    return true;
                }
            }

            if (token.Kind == SyntaxKind.CloseBraceToken &&
                token.Parent.IsKind(SyntaxKind.SwitchStatement))
            {
                return true;
            }

            if (token.Kind == SyntaxKind.ColonToken)
            {
                if (token.Parent.IsKind(SyntaxKind.CaseSwitchLabel, SyntaxKind.DefaultSwitchLabel))
                {
                    return true;
                }
            }

            if (token.Kind == SyntaxKind.DoKeyword &&
                token.Parent.IsKind(SyntaxKind.DoStatement))
            {
                return true;
            }

            if (token.Kind == SyntaxKind.CloseParenToken)
            {
                var parent = token.Parent;
                if (parent.IsKind(SyntaxKind.ForStatement) ||
                    parent.IsKind(SyntaxKind.WhileStatement) ||
                    parent.IsKind(SyntaxKind.IfStatement))
                {
                    return true;
                }
            }

            if (token.Kind == SyntaxKind.ElseKeyword)
            {
                return true;
            }

            return false;
        }

        public static bool IsSwitchLabelContext(this SyntaxToken targetToken)
        {
            // cases:
            //   case X: |
            //   default: |
            //   switch (e) { |
            //
            //   case X: Statement(); |

            if (targetToken.Kind == SyntaxKind.OpenBraceToken &&
                targetToken.Parent.IsKind(SyntaxKind.SwitchStatement))
            {
                return true;
            }

            if (targetToken.Kind == SyntaxKind.ColonToken)
            {
                if (targetToken.Parent.IsKind(SyntaxKind.CaseSwitchLabel, SyntaxKind.DefaultSwitchLabel))
                {
                    return true;
                }
            }

            if (targetToken.Kind == SyntaxKind.SemiToken ||
                targetToken.Kind == SyntaxKind.CloseBraceToken)
            {
                var section = targetToken.GetAncestor<SwitchSectionSyntax>();
                if (section != null)
                {
                    foreach (var statement in section.Statements)
                    {
                        if (targetToken == statement.GetLastToken(includeSkippedTokens: true))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}