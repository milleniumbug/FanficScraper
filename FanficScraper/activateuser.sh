#!/bin/bash
sqlite3 app.db -cmd ".parameter set @id ""'$1'""" 'UPDATE Users SET IsActivated = 1 WHERE Id = @id' 