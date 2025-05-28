// Kanban Drag and Drop functionality
window.kanbanDragDrop = {
    draggedTaskId: null,
    draggedFromColumnId: null,

    // Инициализация drag-and-drop для задачи
    initTaskDragDrop: function (taskElementId, taskId, columnId) {
        console.log(`Инициализация drag для задачи: ${taskElementId}, ID: ${taskId}, колонка: ${columnId}`);
        
        const taskElement = document.getElementById(taskElementId);
        if (!taskElement) {
            console.warn(`Task element not found: ${taskElementId}`);
            return;
        }

        console.log(`Элемент найден: ${taskElementId}`);
        taskElement.draggable = true;
        
        // Удаляем старые обработчики если есть
        if (taskElement._dragStartHandler) {
            taskElement.removeEventListener('dragstart', taskElement._dragStartHandler);
            taskElement.removeEventListener('dragend', taskElement._dragEndHandler);
        }
        
        // Создаем новые обработчики
        taskElement._dragStartHandler = function (e) {
            console.log(`Начало перетаскивания задачи: ${taskId}`);
            kanbanDragDrop.draggedTaskId = taskId;
            kanbanDragDrop.draggedFromColumnId = columnId;
            
            // Добавляем стили для перетаскиваемого элемента
            e.target.style.opacity = '0.5';
            e.target.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/html', e.target.outerHTML);
            
            // Предотвращаем клик после drag
            setTimeout(() => {
                e.target.style.pointerEvents = 'none';
            }, 0);
        };

        taskElement._dragEndHandler = function (e) {
            console.log(`Конец перетаскивания задачи: ${taskId}`);
            // Восстанавливаем стили
            e.target.style.opacity = '1';
            e.target.classList.remove('dragging');
            
            // Восстанавливаем события через небольшую задержку
            setTimeout(() => {
                e.target.style.pointerEvents = 'auto';
            }, 100);
            
            kanbanDragDrop.draggedTaskId = null;
            kanbanDragDrop.draggedFromColumnId = null;
        };
        
        // Добавляем обработчики
        taskElement.addEventListener('dragstart', taskElement._dragStartHandler);
        taskElement.addEventListener('dragend', taskElement._dragEndHandler);
        
        console.log(`Drag-and-drop настроен для задачи: ${taskId}`);
    },

    // Инициализация drop zone для колонки
    initColumnDropZone: function (columnElementId, columnId, dotNetRef) {
        console.log(`Инициализация drop zone для колонки: ${columnElementId}, ID: ${columnId}`);
        
        const columnElement = document.getElementById(columnElementId);
        if (!columnElement) {
            console.warn(`Column element not found: ${columnElementId}`);
            return;
        }

        console.log(`Элемент колонки найден: ${columnElementId}`);

        // Удаляем старые обработчики если есть
        if (columnElement._dragOverHandler) {
            columnElement.removeEventListener('dragover', columnElement._dragOverHandler);
            columnElement.removeEventListener('dragleave', columnElement._dragLeaveHandler);
            columnElement.removeEventListener('drop', columnElement._dropHandler);
        }
        
        // Создаем новые обработчики
        columnElement._dragOverHandler = function (e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            
            // Добавляем визуальную подсветку
            columnElement.classList.add('drag-over');
        };

        columnElement._dragLeaveHandler = function (e) {
            // Убираем подсветку только если покидаем колонку полностью
            if (!columnElement.contains(e.relatedTarget)) {
                columnElement.classList.remove('drag-over');
            }
        };

        columnElement._dropHandler = function (e) {
            e.preventDefault();
            columnElement.classList.remove('drag-over');
            
            console.log(`Drop в колонку: ${columnId}, перетаскиваемая задача: ${kanbanDragDrop.draggedTaskId}, из колонки: ${kanbanDragDrop.draggedFromColumnId}`);
            
            if (kanbanDragDrop.draggedTaskId && kanbanDragDrop.draggedFromColumnId !== columnId) {
                console.log(`Вызываем Blazor метод для перемещения задачи`);
                // Вызываем метод Blazor для обновления данных
                dotNetRef.invokeMethodAsync('OnTaskDropped', 
                    kanbanDragDrop.draggedTaskId, 
                    kanbanDragDrop.draggedFromColumnId, 
                    columnId);
            } else {
                console.log(`Перемещение отменено: задача уже в этой колонке или нет активной задачи`);
            }
        };
        
        // Добавляем обработчики
        columnElement.addEventListener('dragover', columnElement._dragOverHandler);
        columnElement.addEventListener('dragleave', columnElement._dragLeaveHandler);
        columnElement.addEventListener('drop', columnElement._dropHandler);
        
        console.log(`Drop zone настроен для колонки: ${columnId}`);
    },

    // Очистка всех обработчиков
    cleanup: function () {
        console.log('Очистка drag-and-drop обработчиков');
        kanbanDragDrop.draggedTaskId = null;
        kanbanDragDrop.draggedFromColumnId = null;
        
        // Очищаем все обработчики drag-and-drop
        const tasks = document.querySelectorAll('.kanban-task');
        tasks.forEach(task => {
            if (task._dragStartHandler) {
                task.removeEventListener('dragstart', task._dragStartHandler);
                task.removeEventListener('dragend', task._dragEndHandler);
                task._dragStartHandler = null;
                task._dragEndHandler = null;
            }
        });
        
        const columns = document.querySelectorAll('.kanban-column');
        columns.forEach(column => {
            if (column._dragOverHandler) {
                column.removeEventListener('dragover', column._dragOverHandler);
                column.removeEventListener('dragleave', column._dragLeaveHandler);
                column.removeEventListener('drop', column._dropHandler);
                column._dragOverHandler = null;
                column._dragLeaveHandler = null;
                column._dropHandler = null;
            }
        });
    }
};

// Проверяем, что объект создан
console.log('kanbanDragDrop объект инициализирован:', window.kanbanDragDrop); 