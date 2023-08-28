# Contributing to KISS.HttpClientAuthentication

We welcome your contributions! :heart:

## Submitting a bug report (as an issue)

If you think you've found a bug, please start by searching [GitHub issues](https://github.com/dozer75/Moq.Logger/issues) 
(both open and closed ones!) to see if the problem has already been addressed or documented in any way.

If you find nothing of relevance, [open a new issue](https://github.com/dozer75/Moq.Logger/issues/new).

**What to include:** Try to include the following information in it:

 * A brief summary.
 * Minimal, complete, and verifiable repro code (similar to what you'd have to prepare for a Stack Overflow question; 
   see e.g. [their documentation](https://stackoverflow.com/help/mcve).
 * A description of the expected (correct) outcome.
 * A description of the actual (incorrect) outcome.
 * KISS.HttpClientAuthentication version used.
 * .NET version used.

## Submitting a pull request (PR)

Please open an issue first to discuss the PR you're about to submit with the team.

**What to include:** To submit a pull request please follow the following guideline:

 * The branch name shall be *your GitHub username*/*issue id* all *lowercase*
	* e.g. `dozer75/1234`.
 * All commits shall be descriptive and follow 
   Chris Beams [How to Write a Git Commit Message](https://cbea.ms/git-commit/) post,
   * be sure that the commit contains issue number(s).
   * a PR with badly written comments will be rejected.
 * All commits shall include issue number that are related.
 * Keep your commit diffs free of whitespace-only changes.
 * Keep the number of commits as few as possible,
	* if it is a small change keep it to one commit.
 * All changes shall have unit tests associated with them.

**When a reviewer requests changes to your PR,** we encourage (but don't require) you to keep your PR tidy using 
any Git facilities available: You can rewrite your PR branch's history by amending existing commits, rebasing, 
etc., then force-pushing the changed commits to your PR branch. If you are not familiar enough with Git to do all 
of this, simply adding more commits to make requested changes is fine. A team member merging your PR may decide 
to "squash" them into a single commit to keep the repository's history more easily readable.

## Code style rules

The project includes a [`.editorconfig`](https://editorconfig.org/) file. Please make sure your IDE can read this 
to its full so that all code style requirements are followed across the whole code base.

**Copyright notice:** If you add a new file to the solution, please reproduce the short copyright notice found at 
the beginning of existing files.
