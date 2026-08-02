# WindowsFormsLifetime Copilot Instructions

## Scope and compatibility

- Treat `src/WindowsFormsLifetime` as the published library. Keep its supported target frameworks as `net8.0-windows`, `net9.0-windows`, and `net10.0-windows`.
- Keep each target framework's `Microsoft.Extensions.Hosting` reference in its matching conditional `ItemGroup`.
- Do not change the package `<Version>` unless the user explicitly asks to prepare a release or bump the package version.
- Preserve the public API surface unless the requested change requires an intentional API addition or breaking change.

## Preserve the existing style

- Make surgical edits. Do not reformat unrelated code, reorder existing `using` directives, normalize whitespace, or change a file's established tabs-versus-spaces indentation.
- Follow the local C# style: file-scoped namespaces, PascalCase public members, camelCase parameters, underscore-prefixed private fields, braces for multi-statement blocks, and expression-bodied members only for simple forwarding logic.
- Do not use `var`. Declare local variables with explicit types and prefer target-typed `new()` or `new(arguments)` when the target type is clear.
- Continue using XML documentation for public APIs where the surrounding code documents them. Match the existing concise summaries and parameter descriptions.
- Keep project XML grouped and indented like the file being edited. Do not move unrelated properties or package references.

## Windows Forms and hosting behavior

- Preserve the UI-thread boundary. Create and interact with forms through the synchronization-context and form-provider abstractions rather than bypassing them from background services.
- Preserve DI lifetimes and disposal behavior when adding forms, hosted services, or registrations.
- Add or update xUnit tests for behavior changes. Keep host-related tests in the existing non-parallel collection when they exercise the Windows Forms lifetime.

## Dependencies and validation

- Prefer framework-compatible, stable package versions. Update only the dependencies needed for the requested change and account for API changes in samples and tests.
- Build the solution and run the targeted test project after code or project-file changes.
