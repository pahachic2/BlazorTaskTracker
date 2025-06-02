// Kanban Drag and Drop functionality
window.kanbanDragDrop = {
    draggedTaskId: null,
    draggedFromColumnId: null,
    isInitialized: false,

    // Инициализация drag-and-drop для задачи
    initTaskDragDrop: function (taskElementId, taskId, columnId) {
        console.log(`🎯 JS: Инициализация drag для задачи: ${taskElementId}, ID: ${taskId}, колонка: ${columnId}`);
        
        const taskElement = document.getElementById(taskElementId);
        if (!taskElement) {
            console.error(`❌ JS: Task element не найден: ${taskElementId}`);
            // Попробуем найти все элементы с классом kanban-task
            const allTasks = document.querySelectorAll('.kanban-task');
            console.log(`🔍 JS: Найдено ${allTasks.length} элементов с классом kanban-task:`, Array.from(allTasks).map(el => el.id));
            return;
        }

        console.log(`✅ JS: Элемент найден: ${taskElementId}`, taskElement);
        
        // Проверяем, что элемент видим
        const rect = taskElement.getBoundingClientRect();
        console.log(`📐 JS: Размеры элемента ${taskElementId}:`, rect);
        
        taskElement.draggable = true;
        console.log(`🔧 JS: Установлен draggable=true для ${taskElementId}`);
        
        // Удаляем старые обработчики если есть
        if (taskElement._dragStartHandler) {
            taskElement.removeEventListener('dragstart', taskElement._dragStartHandler);
            taskElement.removeEventListener('dragend', taskElement._dragEndHandler);
            console.log(`🧹 JS: Удалены старые обработчики для ${taskElementId}`);
        }
        
        // Создаем новые обработчики
        taskElement._dragStartHandler = function (e) {
            console.log(`🚀 JS: DRAGSTART - Начало перетаскивания задачи: ${taskId}`);
            kanbanDragDrop.draggedTaskId = taskId;
            const currentColumn = e.target.closest('.kanban-column');
            if (currentColumn) {
                kanbanDragDrop.draggedFromColumnId = currentColumn.id.replace('column-', '');
            } else {
                kanbanDragDrop.draggedFromColumnId = columnId;
            }
            
            // Добавляем стили для перетаскиваемого элемента
            e.target.style.opacity = '0.5';
            e.target.classList.add('dragging');
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/html', e.target.outerHTML);
            
            console.log(`📦 JS: Данные drag установлены:`, {
                taskId: kanbanDragDrop.draggedTaskId,
                fromColumn: kanbanDragDrop.draggedFromColumnId
            });
        };

        taskElement._dragEndHandler = function (e) {
            console.log(`🏁 JS: DRAGEND - Конец перетаскивания задачи: ${taskId}`);
            // Восстанавливаем стили
            e.target.style.opacity = '1';
            e.target.classList.remove('dragging');
            
            kanbanDragDrop.draggedTaskId = null;
            kanbanDragDrop.draggedFromColumnId = null;
            console.log(`🧹 JS: Очищены данные drag`);
        };
        
        // Добавляем обработчики
        taskElement.addEventListener('dragstart', taskElement._dragStartHandler);
        taskElement.addEventListener('dragend', taskElement._dragEndHandler);
        
        // Добавляем обработчик наведения мыши для отладки
        taskElement.addEventListener('mouseenter', function() {
            console.log(`🐭 JS: Наведение мыши на задачу ${taskId} (${taskElementId})`);
        });
        
        console.log(`✅ JS: Drag-and-drop настроен для задачи: ${taskId}`);
    },

    // Инициализация drop zone для колонки
    initColumnDropZone: function (columnElementId, columnId, dotNetRef) {
        console.log(`🏗️ JS: Инициализация drop zone для колонки: ${columnElementId}, ID: ${columnId}`);
        
        const columnElement = document.getElementById(columnElementId);
        if (!columnElement) {
            console.error(`❌ JS: Column element не найден: ${columnElementId}`);
            // Попробуем найти все колонки
            const allColumns = document.querySelectorAll('.kanban-column');
            console.log(`🔍 JS: Найдено ${allColumns.length} элементов с классом kanban-column:`, Array.from(allColumns).map(el => el.id));
            return;
        }

        console.log(`✅ JS: Элемент колонки найден: ${columnElementId}`, columnElement);

        // Удаляем старые обработчики если есть
        if (columnElement._dragOverHandler) {
            columnElement.removeEventListener('dragover', columnElement._dragOverHandler);
            columnElement.removeEventListener('dragleave', columnElement._dragLeaveHandler);
            columnElement.removeEventListener('drop', columnElement._dropHandler);
            console.log(`🧹 JS: Удалены старые обработчики для колонки ${columnElementId}`);
        }
        
        // Создаем новые обработчики
        columnElement._dragOverHandler = function (e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            
            // Добавляем визуальную подсветку
            columnElement.classList.add('drag-over');
            console.log(`🎨 JS: DRAGOVER - Подсветка колонки ${columnId}`);
        };

        columnElement._dragLeaveHandler = function (e) {
            // Убираем подсветку только если покидаем колонку полностью
            if (!columnElement.contains(e.relatedTarget)) {
                columnElement.classList.remove('drag-over');
                console.log(`🎨 JS: DRAGLEAVE - Убрана подсветка колонки ${columnId}`);
            }
        };

        columnElement._dropHandler = function (e) {
            console.log(`🎯 JS: DROP событие получено в колонку: ${columnId}`, e);
            
            // ВАЖНО: Предотвращаем браузерное поведение по умолчанию
            e.preventDefault();
            e.stopPropagation();
            
            // Убираем подсветку
            columnElement.classList.remove('drag-over');
            
            console.log(`📦 JS: Текущие данные drag при drop:`, {
                taskId: kanbanDragDrop.draggedTaskId,
                fromColumn: kanbanDragDrop.draggedFromColumnId,
                toColumn: columnId,
                eventType: e.type
            });
            
            // Проверяем, есть ли активная задача для перемещения
            if (!kanbanDragDrop.draggedTaskId) {
                console.log(`⚠️ JS: Нет активной задачи для перемещения`);
                return;
            }
            
            // Проверяем, не перемещаем ли в ту же колонку
            if (kanbanDragDrop.draggedFromColumnId === columnId) {
                console.log(`⚠️ JS: Перемещение в ту же колонку ${columnId}, отменяем`);
                return;
            }
            
            console.log(`🚀 JS: Условия выполнены, вызываем Blazor метод OnTaskDropped`);
            
            try {
                if (dotNetRef && dotNetRef.invokeMethodAsync) {
                    console.log(`📞 JS: Вызываем dotNetRef.invokeMethodAsync...`);
                    
                    dotNetRef.invokeMethodAsync('OnTaskDropped', 
                        kanbanDragDrop.draggedTaskId, 
                        kanbanDragDrop.draggedFromColumnId, 
                        columnId)
                        .then(() => {
                            console.log(`✅ JS: Blazor метод OnTaskDropped выполнен успешно`);
                        })
                        .catch((error) => {
                            console.error(`❌ JS: Ошибка при выполнении Blazor метода:`, error);
                        });
                        
                    // Обновляем атрибут через небольшую задержку
                    setTimeout(() => {
                        kanbanDragDrop.updateTaskColumnId(kanbanDragDrop.draggedTaskId, columnId);
                    }, 100);
                } else {
                    console.log(`⚠️ JS: dotNetRef недоступен (${!dotNetRef ? 'null' : 'no invokeMethodAsync'}), используем fallback`);
                    // Fallback: просто перемещаем элемент в DOM
                    kanbanDragDrop.moveTaskInDOM(kanbanDragDrop.draggedTaskId, kanbanDragDrop.draggedFromColumnId, columnId);
                }
            } catch (error) {
                console.error(`❌ JS: Исключение при вызове Blazor метода:`, error);
            }
        };
        
        // Добавляем обработчики
        columnElement.addEventListener('dragover', columnElement._dragOverHandler);
        columnElement.addEventListener('dragleave', columnElement._dragLeaveHandler);
        columnElement.addEventListener('drop', columnElement._dropHandler);
        
        console.log(`✅ JS: Drop zone настроен для колонки: ${columnId}`);
    },

    // НОВЫЙ МЕТОД: Обновление data-column-id атрибута задачи
    updateTaskColumnId: function(taskId, newColumnId) {
        console.log(`🔄 JS: Обновление data-column-id для задачи ${taskId} на ${newColumnId}`);
        const taskElement = document.getElementById(`task-${taskId}`);
        if (taskElement) {
            taskElement.setAttribute('data-column-id', newColumnId);
            console.log(`✅ JS: Атрибут data-column-id обновлен для задачи ${taskId}`);
        }
    },

    // Fallback метод для перемещения задач в DOM
    moveTaskInDOM: function(taskId, fromColumnId, toColumnId) {
        console.log(`🔄 JS: Fallback перемещение задачи ${taskId} из ${fromColumnId} в ${toColumnId}`);
        
        const taskElement = document.getElementById(`task-${taskId}`);
        const targetColumn = document.getElementById(`column-${toColumnId}`);
        
        if (taskElement && targetColumn) {
            const taskList = targetColumn.querySelector('.space-y-3');
            if (taskList) {
                taskList.appendChild(taskElement);
                kanbanDragDrop.updateTaskColumnId(taskId, toColumnId);
                console.log(`✅ JS: Задача перемещена в DOM`);
            }
        }
    },

    // Автоматическая инициализация всех элементов
    autoInitialize: function() {
        console.log(`🤖 JS: Автоматическая инициализация drag-and-drop...`);
        
        if (kanbanDragDrop.isInitialized) {
            console.log(`⚠️ JS: Уже инициализировано, пропускаем`);
            return;
        }
        
        const tasks = document.querySelectorAll('.kanban-task');
        const columns = document.querySelectorAll('.kanban-column');
        
        console.log(`🔍 JS: Найдено ${tasks.length} задач и ${columns.length} колонок для автоинициализации`);
        
        // Инициализируем колонки
        columns.forEach(column => {
            const columnId = column.id.replace('column-', '');
            kanbanDragDrop.initColumnDropZone(column.id, columnId, null);
        });
        
        // Инициализируем задачи
        tasks.forEach(task => {
            const taskId = task.getAttribute('data-task-id');
            const columnId = task.getAttribute('data-column-id');
            if (taskId && columnId) {
                kanbanDragDrop.initTaskDragDrop(task.id, taskId, columnId);
            }
        });
        
        kanbanDragDrop.isInitialized = true;
        console.log(`✅ JS: Автоинициализация завершена`);
    },

    // Очистка всех обработчиков
    cleanup: function () {
        console.log('🧹 JS: Очистка drag-and-drop обработчиков');
        kanbanDragDrop.draggedTaskId = null;
        kanbanDragDrop.draggedFromColumnId = null;
        kanbanDragDrop.isInitialized = false;
        
        // Останавливаем MutationObserver
        if (kanbanDragDrop.observer) {
            kanbanDragDrop.observer.disconnect();
            kanbanDragDrop.observer = null;
            console.log('🧹 JS: MutationObserver остановлен');
        }
        
        // Очищаем все обработчики drag-and-drop
        const tasks = document.querySelectorAll('.kanban-task');
        console.log(`🧹 JS: Найдено ${tasks.length} задач для очистки`);
        tasks.forEach(task => {
            if (task._dragStartHandler) {
                task.removeEventListener('dragstart', task._dragStartHandler);
                task.removeEventListener('dragend', task._dragEndHandler);
                task._dragStartHandler = null;
                task._dragEndHandler = null;
            }
        });
        
        const columns = document.querySelectorAll('.kanban-column');
        console.log(`🧹 JS: Найдено ${columns.length} колонок для очистки`);
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
        
        console.log('✅ JS: Очистка завершена');
    },

    // Новый метод: настройка наблюдателя за изменениями DOM
    setupDOMObserver: function() {
        if (kanbanDragDrop.observer) {
            console.log('⚠️ JS: MutationObserver уже настроен');
            return;
        }

        console.log('👀 JS: Настраиваем MutationObserver для отслеживания изменений DOM');
        
        kanbanDragDrop.observer = new MutationObserver(function(mutations) {
            let shouldReinitialize = false;
            
            mutations.forEach(function(mutation) {
                // Проверяем добавление новых узлов
                if (mutation.type === 'childList') {
                    mutation.addedNodes.forEach(function(node) {
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            // Проверяем, добавились ли новые задачи или колонки
                            if (node.classList && (node.classList.contains('kanban-task') || node.classList.contains('kanban-column'))) {
                                console.log(`🔍 JS: Обнаружен новый элемент:`, node.className, node.id);
                                shouldReinitialize = true;
                            }
                            
                            // Также проверяем вложенные элементы
                            const newTasks = node.querySelectorAll && node.querySelectorAll('.kanban-task');
                            const newColumns = node.querySelectorAll && node.querySelectorAll('.kanban-column');
                            if ((newTasks && newTasks.length > 0) || (newColumns && newColumns.length > 0)) {
                                console.log(`🔍 JS: Обнаружены новые вложенные элементы: ${newTasks ? newTasks.length : 0} задач, ${newColumns ? newColumns.length : 0} колонок`);
                                shouldReinitialize = true;
                            }
                        }
                    });
                }
            });
            
            if (shouldReinitialize) {
                console.log('🔄 JS: MutationObserver запускает переинициализацию drag-and-drop');
                // Небольшая задержка для завершения рендеринга
                setTimeout(() => {
                    kanbanDragDrop.forceReinitialize();
                }, 300);
            }
        });
        
        // Начинаем наблюдение за изменениями в document.body
        kanbanDragDrop.observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: false
        });
        
        console.log('✅ JS: MutationObserver настроен и активен');
    },

    // Принудительная переинициализация
    forceReinitialize: function() {
        console.log('🔄 JS: Принудительная переинициализация drag-and-drop');
        
        const tasks = document.querySelectorAll('.kanban-task');
        const columns = document.querySelectorAll('.kanban-column');
        
        console.log(`🔍 JS: Найдено для переинициализации: ${tasks.length} задач и ${columns.length} колонок`);
        
        // Инициализируем колонки
        columns.forEach(column => {
            const columnId = column.id.replace('column-', '');
            if (columnId) {
                kanbanDragDrop.initColumnDropZone(column.id, columnId, null);
            }
        });
        
        // Инициализируем задачи
        tasks.forEach(task => {
            const taskId = task.getAttribute('data-task-id');
            const columnId = task.getAttribute('data-column-id');
            if (taskId && columnId) {
                kanbanDragDrop.initTaskDragDrop(task.id, taskId, columnId);
            }
        });
        
        console.log('✅ JS: Принудительная переинициализация завершена');
    },

    // Проверка готовности элементов
    waitForElement: function(selector, timeout = 5000) {
        return new Promise((resolve, reject) => {
            const element = document.querySelector(selector);
            if (element) {
                resolve(element);
                return;
            }
            
            const observer = new MutationObserver(() => {
                const element = document.querySelector(selector);
                if (element) {
                    observer.disconnect();
                    resolve(element);
                }
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
            
            setTimeout(() => {
                observer.disconnect();
                reject(new Error(`Элемент ${selector} не найден за ${timeout}мс`));
            }, timeout);
        });
    }
};

// Проверяем, что объект создан
console.log('🚀 JS: kanbanDragDrop объект инициализирован:', window.kanbanDragDrop);

// Добавляем глобальную проверку DOM
document.addEventListener('DOMContentLoaded', function() {
    console.log('📄 JS: DOM загружен');
    
    // Запускаем MutationObserver
    kanbanDragDrop.setupDOMObserver();
    
    setTimeout(() => {
        const tasks = document.querySelectorAll('.kanban-task');
        const columns = document.querySelectorAll('.kanban-column');
        console.log(`🔍 JS: После загрузки DOM найдено ${tasks.length} задач и ${columns.length} колонок`);
        
        // Автоматически инициализируем drag-and-drop через 2 секунды, если Blazor не сделал этого
        setTimeout(() => {
            if (!kanbanDragDrop.isInitialized) {
                console.log(`⏰ JS: Blazor не инициализировал drag-and-drop, запускаем автоинициализацию...`);
                kanbanDragDrop.autoInitialize();
            }
        }, 2000);
    }, 1000);
});

// Добавляем глобальную функцию для ручного вызова переинициализации
window.reinitializeDragDrop = function() {
    console.log('🔧 JS: Ручной вызов переинициализации drag-and-drop');
    kanbanDragDrop.forceReinitialize();
}; 