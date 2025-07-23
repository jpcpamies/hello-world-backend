## Standard Workflow

1. First think through the problem, read the codebase for relevant files, and write a plan to `projectplan.md`  
2. The plan should include a list of todo items that you can check off as you complete them  
3. Before you begin working, check in with me and I will verify the plan  
4. Then, begin working on the todo items, marking them as complete as you go  
5. Please, every step of the way, give me a high-level explanation of what changes you made  
6. Make every task and code change as simple as possible. Avoid massive or complex changes. Each change should impact as little code as possible. Everything is about simplicity  
7. When done, add a review section at the end of `projectplan.md` with a summary of the changes you made and any other relevant notes  
8. The database schema in `projectplan.md` is the immutable source of truth for all data structures. Always reference and validate against this schema before implementing any data-related components  
9. Before writing forms, APIs, or data components, ensure all fields match exactly with the database schema properties and data types. Clarify any discrepancies first  
10. Only prioritize database schema definition when the app requires significant data persistence or complex structures. For simple apps, focus first on core functionality  
11. When the project involves substantial data management, complete and validate the database schema before any dependent development begins  
12. Every code change involving data manipulation must maintain strict consistency with the approved schema in `projectplan.md`